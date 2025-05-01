using BankAccount.Writer.DomainEvents;
using Newtonsoft.Json;

namespace BankAccount.Writer.Repositories;

public class AccountDomainEventDeserializer
{
    internal IAccountDomainEvent Deserialize(AccountEventEntity accountEventEntity)
    {
        return accountEventEntity.EventType switch
        {
            nameof(AccountCreatedEvent) => JsonConvert.DeserializeObject<AccountCreatedEvent>(accountEventEntity.Data),
            _ => throw new InvalidOperationException($"Unknown event type: {accountEventEntity.EventType}"),
        };
    }
}
