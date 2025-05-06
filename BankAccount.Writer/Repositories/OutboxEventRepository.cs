using BankAccount.Writer.AccountLogic;
using Dapper;
using Dapper.Bulk;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace BankAccount.Writer.Repositories;

public class OutboxEventRepository : IOutboxEventRepository
{
    private readonly SqlConnection _dbConnection;
    private readonly SqlTransaction _transaction;

    public OutboxEventRepository(SqlConnection dbConnection, SqlTransaction transaction = null)
    {
        _dbConnection = dbConnection;
        _transaction = transaction;
    }

    public async Task SaveAsync(IReadOnlyCollection<VersionedDomainEvent> versionedEvents)
    {
        if (versionedEvents == null || versionedEvents.Count == 0)
            return;

        var outboxEvents = versionedEvents.Select(Map).ToList();
        
        await _dbConnection.BulkInsertAsync(outboxEvents, _transaction).ConfigureAwait(false);        
    }

    public async Task<IEnumerable<OutboxEventEntity>> GetUnProcessedEventsAsync(int batchSize)
    {
        var unProcessedSql = $"SELECT TOP {batchSize} * FROM dbo.OutboxEvents WHERE Published = 0";
        return await _dbConnection.QueryAsync<OutboxEventEntity>(unProcessedSql, _transaction).ConfigureAwait(false);
    }

    public async Task MarkAsProcessedAsync(OutboxEventEntity outbox)
    {
        var updateProcessedSql = "UPDATE dbo.OutboxEvents SET Published = 1 WHERE Version = @Version AND EventType = @EventType";
        await _dbConnection.ExecuteAsync(updateProcessedSql, new { outbox.Version, outbox.EventType }, _transaction).ConfigureAwait(false);
    }

    private OutboxEventEntity Map(VersionedDomainEvent versionedEvent)
    {
        var domainEvent = versionedEvent.DomainEvent;

        return new OutboxEventEntity
        {
            Version = versionedEvent.Version,
            Data = JsonConvert.SerializeObject(domainEvent),
            EventType = domainEvent.GetType().Name,
            Published = false
        };        
    }
}
