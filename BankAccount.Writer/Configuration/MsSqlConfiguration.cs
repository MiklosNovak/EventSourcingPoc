namespace BankAccount.Writer.Configuration;

public record MsSqlConfiguration
{
    public const string SectionName = "MsSql";

    public string Host { get; init; }

    public int Port { get; init; } = 1433;

    public string Database { get; init; }

    public string User { get; init; }

    public string Password { get; init; }

    public string TimeOutInSeconds { get; init; } = "60";

    public string GetConnectionString => $"Server={Host},{Port};Database={Database};User Id={User};Password={Password};TrustServerCertificate=True;Connect Timeout={TimeOutInSeconds};";
}

