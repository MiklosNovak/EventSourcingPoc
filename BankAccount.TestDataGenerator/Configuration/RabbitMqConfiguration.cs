namespace BankAccount.TestDataGenerator.Configuration;

public record RabbitMqConfiguration
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; }

    public string User { get; init; }
    
    public string Password { get; init; }

    public string GetConnection => $"amqp://{User}:{Password}@{Host}";
}