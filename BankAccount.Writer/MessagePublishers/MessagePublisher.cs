using BankAccount.Writer.Repositories;
using Newtonsoft.Json.Linq;
using Rebus.Bus;
using Rebus.Messages;

namespace BankAccount.Writer.MessagePublishers;

public class MessagePublisher : BackgroundService
{
    private readonly OutboxEventRepository _outboxEventRepository;
    private readonly ILogger<MessagePublisher> _logger;
    private readonly IBus _bus;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private readonly int _batchSize = 10;

    public MessagePublisher(OutboxEventRepository outboxEventRepository, IBus bus, ILogger<MessagePublisher> logger)
    {
        _outboxEventRepository = outboxEventRepository;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var unProcessedEvents = await _outboxEventRepository.GetUnProcessedEventsAsync(_batchSize).ConfigureAwait(false);

                foreach (var unProcessedEvent in unProcessedEvents)
                {
                    try
                    {
                        await ProcessAsync(unProcessedEvent).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Failed to publish outbox event: {unProcessedEvent.EventType}, version: {unProcessedEvent.Version}";
                        _logger.LogError(exception: ex, message: errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during outbox processing.");
            }

            await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task ProcessAsync(OutboxEventEntity unProcessedEvent)
    {
        // integration event can be totally different, this is done for simplicity        
        var headers = new Dictionary<string, string> { { Headers.Type, unProcessedEvent.EventType } };
        var payload = JObject.Parse(unProcessedEvent.Data);
        payload["Version"] = unProcessedEvent.Version;

        await _bus.Advanced.Topics.Publish(unProcessedEvent.EventType, payload, headers).ConfigureAwait(false);
        await _outboxEventRepository.MarkAsProcessedAsync(unProcessedEvent).ConfigureAwait(false);
    }
}