namespace BankAccount.Writer.MessageHandlers.MoneyWithdrawn;

public record MoneyWithdrawnCommand
{
    public string Email { get; init; }

    public decimal Amount { get; init; }
}
