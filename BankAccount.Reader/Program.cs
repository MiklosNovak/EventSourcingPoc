using BankAccount.Reader;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {                        
        config.AddJsonFile("appsettings.json", true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices(ServiceRegistrations.RegisterServices)
    .UseConsoleLifetime()
    .Build();

await host.RunAsync().ConfigureAwait(false);