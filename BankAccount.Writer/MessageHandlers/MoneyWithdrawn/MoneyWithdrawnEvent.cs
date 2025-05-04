namespace BankAccount.Writer.MessageHandlers.MoneyWithdrawn;

public record MoneyWithdrawnEvent
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }
}
