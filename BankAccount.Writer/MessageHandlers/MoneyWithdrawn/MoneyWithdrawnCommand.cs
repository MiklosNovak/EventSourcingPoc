namespace BankAccount.Writer.MessageHandlers.MoneyWithdrawn;

public record MoneyWithdrawnCommand
{
    public string AccountId { get; init; }

    public decimal Amount { get; init; }
}
