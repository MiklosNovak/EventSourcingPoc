namespace BankAccount.Writer.MessageHandlers.AccountCreatedEvent;

public record AccountCreatedEvent
{
    public string AccountId { get; init; }

    public DateTimeOffset OccurredAt { get; init; }    
}
