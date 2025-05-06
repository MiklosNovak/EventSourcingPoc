namespace BankAccount.TestDataGenerator.IntegrationEvents;

public record AccountStateCorruptedEvent
{
    public string AccountId { get; init; }
}