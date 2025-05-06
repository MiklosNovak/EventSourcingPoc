using BankAccount.Writer.Repositories.OutboxEvents;
using Newtonsoft.Json.Linq;
using Rebus.Bus;
using Rebus.Messages;

namespace BankAccount.Writer.MessagePublishers;

public class MessagePublisher : IMessagePublisher
{
    private readonly IOutboxEventRepository _outboxEventRepository;
    private readonly ILogger<MessagePublisher> _logger;
    private readonly IBus _bus;
    private readonly int _batchSize = 10;

    public MessagePublisher(IOutboxEventRepository outboxEventRepository, IBus bus, ILogger<MessagePublisher> logger)
    {
        _outboxEventRepository = outboxEventRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishMessagesAsync()
    {
        var unProcessedEvents = await _outboxEventRepository.GetUnProcessedAsync(_batchSize).ConfigureAwait(false);

        foreach (var unProcessedEvent in unProcessedEvents)
        {
            try
            {
                await PublishAsync(unProcessedEvent).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorMessage =
                    $"Failed to publish outbox event: {unProcessedEvent.EventType}, version: {unProcessedEvent.Version}";
                _logger.LogError(exception: ex, message: errorMessage);
            }
        }
    }

    private async Task PublishAsync(OutboxEventEntity unProcessedEvent)
    {
        // integration event can be totally different, this is done for simplicity        
        var headers = new Dictionary<string, string> {{Headers.Type, unProcessedEvent.EventType}};
        var payload = JObject.Parse(unProcessedEvent.Data);
        payload["Version"] = unProcessedEvent.Version;

        await _bus.Advanced.Topics.Publish(unProcessedEvent.EventType, payload, headers).ConfigureAwait(false);
        await _outboxEventRepository.MarkAsProcessedAsync(unProcessedEvent).ConfigureAwait(false);
    }
}