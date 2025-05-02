using BankAccount.Writer.DomainEvents;
using Dapper.Bulk;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace BankAccount.Writer.Repositories;

public class OutboxEventRepository
{
    private readonly SqlConnection _dbConnection;
    private readonly SqlTransaction _transaction;

    public OutboxEventRepository(SqlConnection dbConnection, SqlTransaction transaction = null)
    {
        _dbConnection = dbConnection;
        _transaction = transaction;
    }

    public async Task SaveAsync(IEnumerable<VersionedDomainEvent> versionedEvents)
    {
        if (versionedEvents == null || !versionedEvents.Any())
            return;

        var outboxEvents = versionedEvents.Select(Map).ToList();
        
        await _dbConnection.BulkInsertAsync(outboxEvents, _transaction).ConfigureAwait(false);        
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
