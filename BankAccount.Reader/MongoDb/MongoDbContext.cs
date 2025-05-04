using BankAccount.Reader.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BankAccount.Reader.MongoDb;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    private bool _isDisposed;

    public MongoDbContext(IOptions<MongoDbConfiguration> options, IMongoClient mongoClient)
    {
        var mongoDbSettings = options.Value;
        _database = mongoClient.GetDatabase(mongoDbSettings.Database);
    }

    public IMongoCollection<TEntity> Collection<TEntity>(string collectionName) => _database.GetCollection<TEntity>(collectionName);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
    }
}
