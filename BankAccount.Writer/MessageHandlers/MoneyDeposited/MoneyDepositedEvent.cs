namespace BankAccount.Writer.MessageHandlers.MoneyDeposited;

public record MoneyDepositedEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }
}
