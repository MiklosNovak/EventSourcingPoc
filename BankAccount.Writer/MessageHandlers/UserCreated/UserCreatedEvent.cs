namespace BankAccount.Writer.MessageHandlers.UserCreated;

public record UserCreatedEvent
{
    public string AccountId { get; init; }    
}
