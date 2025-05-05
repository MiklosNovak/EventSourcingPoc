namespace BankAccount.Reader.MessageHandlers.AccountStateCorrupted;

public record AccountStateCorruptedEvent
{
    public string AccountId { get; init; }
}
