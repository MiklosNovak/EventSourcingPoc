namespace BankAccount.TestDataGenerator.IntegrationEvents;

public record MoneyDepositedEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }
}
