namespace BankAccount.Reader.MessageHandlers.AccountDebited;

public record AccountDebitedEvent : IIntegrationEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }

    public int Version { get; init; }
}
