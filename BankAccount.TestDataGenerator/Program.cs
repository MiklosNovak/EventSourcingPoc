using BankAccount.TestDataGenerator;
using BankAccount.TestDataGenerator.IntegrationEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
ServiceRegistrations.RegisterServices(configuration, services);
var provider = services.BuildServiceProvider();


var publisher = provider.GetRequiredService<MessagePublisher>();

var events = new List<object>
{
    // Create users
    new UserCreatedEvent { AccountId = "jane@example.com" },
    new UserCreatedEvent { AccountId = "christine@example.com" },
    new UserCreatedEvent { AccountId = "richard@example.com" },

    // Initial deposits
    new MoneyDepositedEvent { AccountId = "jane@example.com", Amount = 1000 },   // jane: 1000
    new MoneyDepositedEvent { AccountId = "christine@example.com", Amount = 500 },      // christine: 500
    new MoneyDepositedEvent { AccountId = "richard@example.com", Amount = 2000 }, // richard: 2000

    // Withdrawals
    new MoneyWithdrawnEvent { AccountId = "jane@example.com", Amount = 200 },    // jane: 800
    new MoneyWithdrawnEvent { AccountId = "richard@example.com", Amount = 300 },  // richard: 1700

    // Transfers
    new MoneyTransferredEvent
    {
        AccountId = "christine@example.com",
        TargetAccountId = "jane@example.com",
        Amount = 150                                        // christine: 350, jane: 950
    },
    new MoneyTransferredEvent
    {
        AccountId = "richard@example.com",
        TargetAccountId = "christine@example.com",
        Amount = 400                                        // richard: 1300, christine: 750
    },

    new AccountStateCorruptedEvent
    {
        AccountId = "richard@example.com"
    }
};

// Publish all events in order
foreach (var evt in events)
{
    await publisher.PublishAsync(evt).ConfigureAwait(false);
    await Task.Delay(1000).ConfigureAwait(false);
}
