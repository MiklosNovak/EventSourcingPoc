using BankAccount.Writer.DomainEvents;
using Newtonsoft.Json;

namespace BankAccount.Writer.Repositories;

internal class AccountDomainEventDeserializer : IAccountDomainEventDeserializer
{
    public IAccountDomainEvent Deserialize(AccountEventEntity accountEventEntity) => accountEventEntity.EventType switch
    {
        nameof(AccountCreatedEvent) => JsonConvert.DeserializeObject<AccountCreatedEvent>(accountEventEntity.Data),
        nameof(AccountCreditedEvent) => JsonConvert.DeserializeObject<AccountCreditedEvent>(accountEventEntity.Data),
        nameof(AccountDebitedEvent) => JsonConvert.DeserializeObject<AccountDebitedEvent>(accountEventEntity.Data),

        _ => throw new InvalidOperationException($"Unknown event type: {accountEventEntity.EventType}"),
    };
}
