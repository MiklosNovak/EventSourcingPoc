namespace BankAccount.Reader.MessageHandlers.AccountCredited;

public record AccountCreditedEvent : IIntegrationEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }

    public int Version { get; init; }
}