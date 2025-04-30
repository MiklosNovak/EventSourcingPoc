namespace BankAccount.Writer.DomainEvents;

public record AccountCreatedEvent : IAccountDomainEvent
{
    public string AccountId { get; init; }

    public DateTimeOffset OccurredAt { get; init; }    
}
