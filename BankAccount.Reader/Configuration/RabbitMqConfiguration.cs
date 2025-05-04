namespace BankAccount.Reader.Configuration;

public record RabbitMqConfiguration
{
    public const string SectionName = "RabbitMq";

    public string Queue => "BankAccountReaderQueue";

    public string ErrorQueue => "BankAccountReaderErrorQueue";

    public string Host { get; init; }

    public string User { get; init; }
    
    public string Password { get; init; }

    public int MaxRetry { get; init; } = 3;

    public string GetConnection => $"amqp://{User}:{Password}@{Host}";
}