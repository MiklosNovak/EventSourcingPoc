namespace BankAccount.TestDataGenerator.IntegrationEvents;

public record MoneyWithdrawnEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }
}
