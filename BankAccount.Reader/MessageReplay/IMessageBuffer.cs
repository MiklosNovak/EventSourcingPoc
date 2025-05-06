using BankAccount.Reader.MessageHandlers;

namespace BankAccount.Reader.MessageReplay;

public interface IMessageBuffer
{
    void Add(IIntegrationEvent message);

    IReadOnlyCollection<IIntegrationEvent> TakeAll();
}