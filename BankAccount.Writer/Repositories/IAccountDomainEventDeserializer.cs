using BankAccount.Writer.DomainEvents;

namespace BankAccount.Writer.Repositories;

internal interface IAccountDomainEventDeserializer
{
    IAccountDomainEvent Deserialize(AccountEventEntity accountEventEntity);
}