using BankAccount.Reader.MessagePublishers;

namespace BankAccount.Reader.MessageReplay;

public class MessageReplayer
{
    private readonly IMessageBuffer _messageBuffer;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(1);
    private readonly IMessagePublisher _messagePublisher;

    public MessageReplayer(IMessageBuffer messageBuffer, IMessagePublisher messagePublisher)
    {
        _messageBuffer = messageBuffer;
        _messagePublisher = messagePublisher;
    }

    public async Task ReplayEventsAsync()
    {
        var isMessageBuffered = _messageBuffer.TryGet(out var message);

        if (!isMessageBuffered)
        {
            return;
        }

        if (IsMessageExpired(message))
        {
            return;
        }

        // for simplicity, I send the message directly to the bus, in a real-world scenario, you might want to use a more sophisticated approach
        await SendMessageAsync(message).ConfigureAwait(false);
    }

    private bool IsMessageExpired(ReplayableEvent message)
    {
        return message.ExpiryDate.HasValue && message.ExpiryDate.Value + _ttl < DateTime.UtcNow;
    }

    private async Task SendMessageAsync(ReplayableEvent message)
    {
        message.ExpiryDate ??= DateTime.UtcNow.Add(_ttl);

        await _messagePublisher.SendLocalAsync(message).ConfigureAwait(false);
    }
}