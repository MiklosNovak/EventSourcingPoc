using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;
using Rebus.Bus;
using Rebus.Messages;

namespace BankAccount.Reader.MessagePublishers;

public class MessagePublisher
{
    private readonly IBus _bus;

    public MessagePublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAccountReplyRequestedEventAsync(AccountStateCorruptedEvent message)
    {
        var eventType = "AccountReplyRequestedEvent";
        var headers = new Dictionary<string, string> { { Headers.Type, eventType } };
        var payload = new { AccountId = message.AccountId };

        await _bus.Advanced.Topics.Publish(eventType, payload, headers).ConfigureAwait(false);
    }
}