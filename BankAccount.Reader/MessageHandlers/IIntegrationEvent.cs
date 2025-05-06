namespace BankAccount.Reader.MessageHandlers;

public interface IIntegrationEvent
{
    public string AccountId { get; }

    public int Version { get; }
}