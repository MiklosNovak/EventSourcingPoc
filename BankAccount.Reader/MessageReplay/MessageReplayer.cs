using Rebus.Bus;
using Rebus.Messages;

namespace BankAccount.Reader.MessageReplay;

public class MessageReplayer : BackgroundService
{
    private readonly IMessageBuffer _messageBuffer;
    private readonly IBus _bus;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(1);
    private readonly ILogger<MessageReplayer> _logger;

    public MessageReplayer(IMessageBuffer messageBuffer, IBus bus, ILogger<MessageReplayer> logger)
    {
        _messageBuffer = messageBuffer;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);

            try
            {
                var isMessageBuffered = _messageBuffer.TryGet(out var message);

                if (!isMessageBuffered)
                {
                    continue;
                }

                if (IsMessageExpired(message))
                {
                    continue;
                }

                // for simplicity, I send the message directly to the bus, in a real-world scenario, you might want to use a more sophisticated approach
                await SendLocalMessageAsync(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during buffer processing.");
            }
        }
    }

    private bool IsMessageExpired(ReplayableEvent message)
    {
        return message.ExpiryDate.HasValue && message.ExpiryDate.Value + _ttl < DateTime.UtcNow;
    }

    private async Task SendLocalMessageAsync(ReplayableEvent message)
    {
        var eventType = message.GetType().Name;
        var headers = new Dictionary<string, string>
        {
            { Headers.Type, eventType }
        };

        message.ExpiryDate ??= DateTime.UtcNow.Add(_ttl);

        await _bus.SendLocal(message, headers).ConfigureAwait(false);
    }
}