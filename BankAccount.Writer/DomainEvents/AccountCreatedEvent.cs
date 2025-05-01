namespace BankAccount.Writer.DomainEvents;

public record AccountCreatedEvent : IAccountDomainEvent
{
    public string AccountId { get; init; }    
}
