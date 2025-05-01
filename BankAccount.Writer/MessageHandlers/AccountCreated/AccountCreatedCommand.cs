namespace BankAccount.Writer.DomainEvents;

public record AccountCreatedCommand
{
    public string AccountId { get; init; }    
}
