using BankAccount.Writer.DomainEvents;

namespace BankAccount.Writer.AccountLogic;

public class VersionedDomainEvent
{
    public IAccountDomainEvent DomainEvent { get; }

    public int Version { get; }

    public VersionedDomainEvent(IAccountDomainEvent accountDomainEvent, int version)
    {
        DomainEvent = accountDomainEvent;
        Version = version;
    }
}