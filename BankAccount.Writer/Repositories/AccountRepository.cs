using BankAccount.Writer.AccountLogic;
using Dapper;
using Dapper.Bulk;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace BankAccount.Writer.Repositories;

public class AccountRepository
{
    private readonly SqlConnection _dbConnection;
    private readonly SqlTransaction _transaction;
    private readonly AccountDomainEventDeserializer _eventDeserializer;

    public AccountRepository(SqlConnection dbConnection, AccountDomainEventDeserializer eventDeserializer, SqlTransaction transaction = null) 
    {
        _dbConnection = dbConnection;
        _eventDeserializer = eventDeserializer;
        _transaction = transaction;
    }

    public async Task<Account> GetAsync(string email)
    {
        const string sql = @"SELECT * FROM dbo.AccountEvents WHERE AccountId = @Email ORDER BY Version ASC;";

        var events = await _dbConnection.QueryAsync<AccountEventEntity>(sql, new { Email = email }, _transaction).ConfigureAwait(false);

        if (events == null || !events.Any())
        {
            return null;
        }

        var domainEvents = events.Select(_eventDeserializer.Deserialize).ToArray();

        return Account.Rehydrate(domainEvents);        
    }

    public async Task SaveAsync(Account account)
    {
        const string getVersionSql = @"SELECT ISNULL(MAX(Version), 0) FROM dbo.AccountEvents WHERE AccountId = @AccountId;";

        var currentMaxVersion = await _dbConnection.ExecuteScalarAsync<int>(getVersionSql, new { account.AccountId }, _transaction).ConfigureAwait(false);
        var expectedVersion = account.Version - account.GetUncommittedEvents.Count;

        // if someone else has inserted an event in the meantime, we will get a concurrency exception
        if (currentMaxVersion != expectedVersion)
        {
            throw new InvalidOperationException($"Concurrency conflict! Expected version: {expectedVersion}, but found: {currentMaxVersion}");
        }

        var domainEvents = account.GetUncommittedEvents.Select((uncommittedEvent, idx) =>
        {
            var domainEvent = uncommittedEvent.DomainEvent; // this is important for the reader

            return new AccountEventEntity
            {
                EventId = Guid.NewGuid(),
                AccountId = domainEvent.AccountId,
                Version = uncommittedEvent.Version,
                EventType = domainEvent.GetType().Name,
                Data = JsonConvert.SerializeObject(domainEvent),
                OccurredAt = DateTime.UtcNow,
                SchemaVersion = 1
            };
        });

        // itt will be atomic, if version conflict occures due to concurrency will throw an exception because of the database constraint
        await _dbConnection.BulkInsertAsync(domainEvents, _transaction).ConfigureAwait(false);        

        account.ClearUncommittedEvents();
    }
}
