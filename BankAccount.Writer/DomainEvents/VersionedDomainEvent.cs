namespace BankAccount.Writer.DomainEvents;

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