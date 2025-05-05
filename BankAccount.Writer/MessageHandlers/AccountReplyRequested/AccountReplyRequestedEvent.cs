namespace BankAccount.Writer.MessageHandlers.AccountReplyRequested;

public record AccountReplyRequestedEvent
{
    public string AccountId { get; init; }    
}
