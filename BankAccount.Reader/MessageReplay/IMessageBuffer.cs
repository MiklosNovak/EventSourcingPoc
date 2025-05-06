namespace BankAccount.Reader.MessageReplay;

public interface IMessageBuffer
{
    void Add(ReplayableEvent message);
    bool TryGet(out ReplayableEvent result);
}