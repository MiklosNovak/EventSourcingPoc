using BankAccount.Reader.MessageHandlers;
using System.Collections.Concurrent;
namespace BankAccount.Reader.MessageReplay;

public class MessageBuffer : IMessageBuffer
{
    private readonly ConcurrentBag<IIntegrationEvent> _buffer = [];

    public void Add(IIntegrationEvent message)
    {
        _buffer.Add(message);
    }

    public IReadOnlyCollection<IIntegrationEvent> TakeAll()
    {
        var messages = new List<IIntegrationEvent>();

        while (_buffer.TryTake(out var message))
        {
            messages.Add(message);
        }

        return messages;
    }
}