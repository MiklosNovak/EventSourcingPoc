namespace BankAccount.Writer.DomainEvents;

public record AccountCreatedCommand
{
    public string Email { get; init; }    
}
