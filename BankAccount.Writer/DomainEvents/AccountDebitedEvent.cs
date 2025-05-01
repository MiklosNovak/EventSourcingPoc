namespace BankAccount.Writer.DomainEvents;
public record AccountDebitedEvent : IAccountDomainEvent
{
    public string AccountId { get; init; }
    public decimal Amount { get; init; }
}
