namespace BankAccount.Writer.DomainEvents;

public record AccountCreditedEvent : IAccountDomainEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }
}
