namespace BankAccount.Writer.Configuration;

public record RabbitMqConfiguration
{
    public const string SectionName = "RabbitMq";

    public string Queue => "BankAccountWriterQueue";

    public string ErrorQueue => "BankAccountWriterErrorQueue";

    public string Host { get; init; }

    public string User { get; init; }
    
    public string Password { get; init; }

    public int MaxRetry { get; init; } = 3;

    public string GetConnection => $"amqp://{User}:{Password}@{Host}";
}
