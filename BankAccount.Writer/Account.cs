using BankAccount.Writer.DomainEvents;

namespace BankAccount.Writer;

public class Account
{    
    private readonly List<IAccountDomainEvent> _uncommittedEvents = [];

    public string AccountId { get; private set; }  
    
    public decimal Balance { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyCollection<IAccountDomainEvent> GetUncommittedEvents => _uncommittedEvents.AsReadOnly();

    public Account(params IAccountDomainEvent[] events)
    {
        foreach (var domainEvent in events)
        {
            Apply(domainEvent);
        }
    }

    public void Apply(IAccountDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case AccountCreatedEvent accountCreatedEvent:
               
                if (!accountCreatedEvent.AccountId.IsValidEmail())
                {
                    throw new ArgumentException("AccountId should be a valid mail address!");
                }

                AccountId = accountCreatedEvent.AccountId.ToLowerInvariant();
                Balance = 0;                

                break;
            default:
                throw new InvalidOperationException($"Unknown event type!");
        }

        _uncommittedEvents.Add(domainEvent);
        Version++;
    }

    public void ClearUncommittedEvents()
    {        
        _uncommittedEvents.Clear();     
    }
}
