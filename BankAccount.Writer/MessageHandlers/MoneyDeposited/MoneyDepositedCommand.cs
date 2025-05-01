namespace BankAccount.Writer.DomainEvents;

public record MoneyDepositedCommand
{
    public string Email { get; init; }

    public decimal Amount { get; init; }
}
