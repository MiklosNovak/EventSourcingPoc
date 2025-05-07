using BankAccount.Reader.MessageHandlers;
using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Messages;

namespace BankAccount.Reader.MessagePublishers;

public class MessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;
    private readonly IServiceProvider _serviceProvider;

    public MessagePublisher(IBus bus, IServiceProvider serviceProvider)
    {
        _bus = bus;
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAccountReplyRequestedEventAsync(AccountStateCorruptedEvent message)
    {
        var eventType = "AccountReplyRequestedEvent";
        var headers = new Dictionary<string, string> { { Headers.Type, eventType } };
        var payload = new { AccountId = message.AccountId };

        await _bus.Advanced.Topics.Publish(eventType, payload, headers).ConfigureAwait(false);
    }

    public async Task SendLocalAsync(IIntegrationEvent message)
    {
        // Since the logic resides in the message handlers and I don't want to re-queue the message, invoking the handlers directly is the simplest solution.
        // In a real - world scenario, however, it's better to separate the business logic from the message handlers.

        var messageType = message.GetType();
        var handlerInterfaceType = typeof(IHandleMessages<>).MakeGenericType(messageType);

        var handlers = _serviceProvider.GetServices(handlerInterfaceType);

        foreach (var handler in handlers)
        {
            var handleMethod = handlerInterfaceType.GetMethod("Handle");
            var task = (Task)handleMethod!.Invoke(handler, [message]);
            await task!.ConfigureAwait(false);
        }
    }
}