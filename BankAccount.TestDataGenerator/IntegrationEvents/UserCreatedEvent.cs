namespace BankAccount.TestDataGenerator.IntegrationEvents;

public record UserCreatedEvent
{
    public string AccountId { get; init; }    
}
