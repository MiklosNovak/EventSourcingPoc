using BankAccount.Writer.AccountLogic;

namespace BankAccount.Writer.Repositories.OutboxEvents;

public interface IOutboxEventRepository
{
    Task AddAsync(IReadOnlyCollection<VersionedDomainEvent> versionedEvents);
    Task<IEnumerable<OutboxEventEntity>> GetUnProcessedAsync(int batchSize);
    Task MarkAsProcessedAsync(OutboxEventEntity outbox);
}