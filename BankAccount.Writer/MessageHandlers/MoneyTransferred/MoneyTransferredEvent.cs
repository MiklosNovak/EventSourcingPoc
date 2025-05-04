namespace BankAccount.Writer.MessageHandlers.MoneyTransferred;

public record MoneyTransferredEvent
{
    public string AccountId { get; init; }

    public string TargetAccountId { get; init; }

    public decimal Amount { get; init; }
}
