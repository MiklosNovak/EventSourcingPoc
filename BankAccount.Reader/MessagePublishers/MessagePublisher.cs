using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;
using BankAccount.Reader.MessageReplay;
using Rebus.Bus;
using Rebus.Messages;

namespace BankAccount.Reader.MessagePublishers;

public class MessagePublisher : IMessagePublisher
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

    public async Task SendLocalAsync(ReplayableEvent message)
    {
        var eventType = message.GetType().Name;
        var headers = new Dictionary<string, string>
        {
            { Headers.Type, eventType }
        };

        await _bus.SendLocal(message, headers).ConfigureAwait(false);
    }
}