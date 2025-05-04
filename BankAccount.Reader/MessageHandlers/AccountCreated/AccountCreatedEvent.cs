namespace BankAccount.Reader.MessageHandlers.AccountCreated;

public record AccountCreatedEvent
{
    public string AccountId { get; init; }

    public int Version { get; init; }
}
