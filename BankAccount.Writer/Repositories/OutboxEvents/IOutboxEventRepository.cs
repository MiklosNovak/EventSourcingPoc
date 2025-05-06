using BankAccount.Writer.AccountLogic;

namespace BankAccount.Writer.Repositories.OutboxEvents;

public interface IOutboxEventRepository
{
    Task SaveAsync(IReadOnlyCollection<VersionedDomainEvent> versionedEvents);
    Task<IEnumerable<OutboxEventEntity>> GetUnProcessedEventsAsync(int batchSize);
    Task MarkAsProcessedAsync(OutboxEventEntity outbox);
}