namespace BankAccount.Reader.Configuration;

public record MongoDbConfiguration
{
    public const string SectionName = "MongoDb";

    public string Host { get; init; }

    public string Database { get; init; }

    public string User { get; init; }

    public string Password { get; init; }

    public string GetConnection => $"mongodb://{User}:{Password}@{Host}";
}
