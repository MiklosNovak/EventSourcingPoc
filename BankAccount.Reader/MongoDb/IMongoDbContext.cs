using MongoDB.Driver;

namespace BankAccount.Reader.MongoDb;

public interface IMongoDbContext : IDisposable
{
    IMongoCollection<TEntity> Collection<TEntity>(string collectionName);
}
