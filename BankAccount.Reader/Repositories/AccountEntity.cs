using MongoDB.Bson.Serialization.Attributes;

namespace BankAccount.Reader.Repositories;

internal class AccountEntity
{
    [BsonIgnore]
    public const string CollectionName = "Account";

    [BsonId]
    public string AccountId { get; init; }

    public decimal Balance { get; init; }

    public int Version { get; init; }
}