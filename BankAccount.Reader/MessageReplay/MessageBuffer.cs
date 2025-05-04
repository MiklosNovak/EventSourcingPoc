using System.Collections.Concurrent;
namespace BankAccount.Reader.MessageReplay;

public class MessageBuffer
{
    private readonly ConcurrentQueue<ReplayableEvent> _buffer = new();

    public void Add(ReplayableEvent message)
    {
        _buffer.Enqueue(message);
    }

    public bool TryGet(out ReplayableEvent result)
    {
        return _buffer.TryDequeue(out result);
    }
}