using BankAccount.Writer.DomainEvents;
using Dapper;
using Dapper.Bulk;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace BankAccount.Writer.Repositories;

public class AccountRepository
{
    private readonly SqlConnection _dbConnection;

    public AccountRepository(SqlConnection dbConnection) 
    {
        _dbConnection = dbConnection;
    }

    public async Task<Account> GetAsync(string email)
    {
        const string sql = @"SELECT * FROM dbo.AccountEvents WHERE AccountId = @Email ORDER BY Version ASC;";

        var events = await _dbConnection.QueryAsync<AccountEventEntity>(sql, new { Email = email }).ConfigureAwait(false);

        if (events == null || !events.Any())
        {
            return null;
        }

        var domainEvents = events.Select(MapToDomainEvent).ToArray();

        return new Account(domainEvents);
    }   

    public async Task AddAsync(Account account)
    {
        const string getVersionSql = @"SELECT ISNULL(MAX(Version), 0) FROM dbo.AccountEvents WHERE AccountId = @AccountId;";

        var currentMaxVersion = await _dbConnection.ExecuteScalarAsync<int>(getVersionSql, new { account.AccountId }).ConfigureAwait(false);                
        var expectedVersion = account.Version - account.GetUncommittedEvents.Count;

        if (currentMaxVersion != expectedVersion)
        {
            throw new InvalidOperationException($"Concurrency conflict! Expected version: {expectedVersion}, but found: {currentMaxVersion}");
        }

        var domainEvents = account.GetUncommittedEvents.Select((domainEvent, idx) =>

            new AccountEventEntity
            {
                EventId = Guid.NewGuid(),
                AccountId = domainEvent.AccountId,
                Version = currentMaxVersion + idx + 1,
                EventType = domainEvent.GetType().Name,
                Data = JsonConvert.SerializeObject(domainEvent),
                OccurredAt = DateTime.UtcNow,
                SchemaVersion = 1
            });

        // itt will be atomic, if version conflict occures due to concurrency, it will throw an exception
        await _dbConnection.BulkInsertAsync(domainEvents).ConfigureAwait(false);        

        account.ClearUncommittedEvents();
    }

    // todo:: extract to separate class    
    private IAccountDomainEvent MapToDomainEvent(AccountEventEntity s)
    {
        return s.EventType switch
        {
            nameof(AccountCreatedEvent) => JsonConvert.DeserializeObject<AccountCreatedEvent>(s.Data),
            _ => throw new InvalidOperationException($"Unknown event type: {s.EventType}"),
        };
    }
}
