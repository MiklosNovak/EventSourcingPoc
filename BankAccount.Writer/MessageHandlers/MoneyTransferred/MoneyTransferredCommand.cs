namespace BankAccount.Writer.DomainEvents;

public record MoneyTransferredCommand
{
    public string AccountId { get; init; }

    public string TargetAccountId { get; init; }

    public decimal Amount { get; init; }
}
