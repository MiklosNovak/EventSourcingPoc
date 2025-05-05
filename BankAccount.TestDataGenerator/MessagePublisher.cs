using Rebus.Bus;
using Rebus.Messages;

namespace BankAccount.TestDataGenerator;

public class MessagePublisher
{
    private readonly IBus _bus;

    public MessagePublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message)
    {
        var eventType = message.GetType().Name;
        var headers = new Dictionary<string, string> { { Headers.Type, eventType } };
        await _bus.Advanced.Topics.Publish(eventType, message, headers).ConfigureAwait(false);
    }
}