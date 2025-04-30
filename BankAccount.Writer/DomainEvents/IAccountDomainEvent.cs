namespace BankAccount.Writer.DomainEvents;

public interface IAccountDomainEvent
{
    public string AccountId { get; }

    public DateTimeOffset OccurredAt { get; }
}
