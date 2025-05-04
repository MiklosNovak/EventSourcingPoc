using BankAccount.Writer.DomainEvents;

namespace BankAccount.Writer.AccountLogic;

public class Account
{    
    private readonly List<VersionedDomainEvent> _uncommittedEvents = [];    

    public string AccountId { get; private set; }  
    
    public decimal Balance { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyCollection<VersionedDomainEvent> GetUncommittedEvents => [.. _uncommittedEvents];
    

    private Account()
    {
    }

    public Account(string email)
    {        
        var accountCreatedEvent = new AccountCreatedEvent
        {
            AccountId = email.ToLowerInvariant()            
        };

        Apply(accountCreatedEvent);
    }

    public void Deposit(decimal amount)
    {
        var accountCreditedEvent = new AccountCreditedEvent
        {            
            Amount = amount,
            AccountId = AccountId
        };

        Apply(accountCreditedEvent);
    }

    public void Withdrawn(decimal amount)
    {
        var accountDebitedEvent = new AccountDebitedEvent
        {
            Amount = amount,
            AccountId = AccountId
        };
        Apply(accountDebitedEvent);
    }

    public static Account Rehydrate(IEnumerable<IAccountDomainEvent> events)
    {
        var account = new Account();
        foreach (var domainEvent in events)
        {
            account.Apply(domainEvent);
        }

        account.ClearUncommittedEvents();
        return account;
    }

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    private void Apply(IAccountDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case AccountCreatedEvent accountCreatedEvent:
                Mutate(accountCreatedEvent);
                break;
            case AccountCreditedEvent accountCreditedEvent:
                Mutate(accountCreditedEvent);
                break;
            case AccountDebitedEvent accountDebitedEvent:
                Mutate(accountDebitedEvent);
                break;
            default:
                throw new InvalidOperationException($"Unknown event type!");
        }

        Version++;

        var versionedDomainEvent = new VersionedDomainEvent(domainEvent, Version);
        _uncommittedEvents.Add(versionedDomainEvent);
    }

    private void Mutate(AccountCreatedEvent accountCreatedEvent)
    {  
        if (!accountCreatedEvent.AccountId.IsValidEmail())
        {
            throw new ArgumentException($"'{nameof(accountCreatedEvent.AccountId)}' should be a valid mail address!");
        }

        AccountId = accountCreatedEvent.AccountId.ToLowerInvariant();
        Balance = 0;
    }

    private void Mutate(AccountCreditedEvent accountCreditedEvent)
    {
        if ( accountCreditedEvent.Amount <= 0)
        {
            throw new ArgumentException($"'{nameof(accountCreditedEvent.Amount)}' should be greater than 0!");
        }        

        if (Balance + accountCreditedEvent.Amount > 10000) {
            throw new InvalidOperationException($"Account '{AccountId}' has reached the maximum balance of 10,000!");
        }
        
        Balance += accountCreditedEvent.Amount;
    }

    private void Mutate(AccountDebitedEvent accountDebitedEvent)
    {
        if (accountDebitedEvent.Amount <= 0)
        {
            throw new ArgumentException($"'{nameof(accountDebitedEvent.Amount)}' should be greater than 0!");
        }

        if (Balance - accountDebitedEvent.Amount < 0)
        {
            throw new InvalidOperationException($"Account '{AccountId}' has insufficient funds!");
        }

        Balance -= accountDebitedEvent.Amount;
    }
}
