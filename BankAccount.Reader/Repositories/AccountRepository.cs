using BankAccount.Reader.AccountLogic;
using BankAccount.Reader.MongoDb;
using MongoDB.Driver;

namespace BankAccount.Reader.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IMongoCollection<AccountEntity> _accountCollection;

    public AccountRepository(IMongoDbContext context)
    {
        _accountCollection = context.Collection<AccountEntity>(AccountEntity.CollectionName);
    }

    public async Task<Account> GetAsync(string accountId)
    {
        var queryResult = await _accountCollection.FindAsync(x => x.AccountId == accountId).ConfigureAwait(false);
            
        var accountEntity = await queryResult.SingleOrDefaultAsync().ConfigureAwait(false);

        if (accountEntity == null)
        {
            return null;
        }

        return new(accountEntity.AccountId, accountEntity.Balance, accountEntity.Version);
    }

    public async Task AddAsync(Account account)
    {
        var accountEntity = new AccountEntity
        {
            AccountId = account.AccountId,
            Balance = account.Balance,
            Version = account.Version
        };

        await _accountCollection.InsertOneAsync(accountEntity).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Account account)
    {
        var replaceResult = await _accountCollection.ReplaceOneAsync(
            x => x.AccountId == account.AccountId && x.Version == account.OldVersion,
            new AccountEntity
            {
                AccountId = account.AccountId,
                Balance = account.Balance,
                Version = account.Version
            },
            new ReplaceOptions
            {
                IsUpsert = false
            }).ConfigureAwait(false);

        if (!replaceResult.IsAcknowledged || replaceResult.ModifiedCount == 0)
        {
            throw new Exception($"Version mismatch for account {account.AccountId}. Expected version: {account.OldVersion}.");
        }
    }

    public async Task DeleteAsync(string accountId)
    {
        await _accountCollection.DeleteOneAsync(x => x.AccountId == accountId).ConfigureAwait(false);
    }
}
