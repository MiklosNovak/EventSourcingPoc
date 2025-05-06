using System.Collections.Concurrent;
using BankAccount.Reader.MessageHandlers;
using BankAccount.Reader.MessagePublishers;

namespace BankAccount.Reader.MessageReplay;

public class MessageReplayer
{
    private readonly IMessageBuffer _messageBuffer;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(1);
    private readonly IMessagePublisher _messagePublisher;
    private readonly ConcurrentDictionary<string, DateTime> _messageHistory = new();

    public MessageReplayer(IMessageBuffer messageBuffer, IMessagePublisher messagePublisher)
    {
        _messageBuffer = messageBuffer;
        _messagePublisher = messagePublisher;
    }

    public async Task ReplayEventsAsync()
    {
        var bufferedMessages = _messageBuffer.TakeAll();

        if (bufferedMessages.Count == 0)
        {
            return;
        }

        // the ordering is important, it can improve the performance of the replays
        var orderedMessages = bufferedMessages
            .OrderBy(m => m.Version);

        foreach (var message in orderedMessages)
        {
            await ReplyMessageAsync(message).ConfigureAwait(false);
        }
    }

    private async Task ReplyMessageAsync(IIntegrationEvent message)
    {
        var messageKey = GetKey(message);

        if (IsMessageExpired(messageKey))
        {
            _messageHistory.TryRemove(messageKey, out _);
            return;
        }

        // for simplicity, I send the message directly to the bus, in a real-world scenario, you might want to use a more sophisticated approach
        await _messagePublisher.SendLocalAsync(message).ConfigureAwait(false);
    }

    private bool IsMessageExpired(string key)
    {
        var expiryDate = _messageHistory.GetOrAdd(key, DateTime.UtcNow.Add(_ttl));

        return expiryDate < DateTime.UtcNow;
    }

    private static string GetKey(IIntegrationEvent message)
    {
        return $"{message.AccountId}_{message.Version}";
    }
}