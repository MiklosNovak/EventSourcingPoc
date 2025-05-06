using BankAccount.Writer.DomainEvents;

namespace BankAccount.Writer.Repositories.Accounts;

internal interface IAccountDomainEventDeserializer
{
    IAccountDomainEvent Deserialize(AccountEventEntity accountEventEntity);
}