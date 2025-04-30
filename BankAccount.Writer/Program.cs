using BankAccount.Writer.Configuration;
using BankAccount.Writer.MessageHandlers.AccountCreatedEvent;
using BankAccount.Writer.MessageProcessing;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Serialization.Json;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {                        
        config.AddJsonFile("appsettings.json", true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var rmqConfiguration = configuration
           .GetRequiredSection(RabbitMqConfiguration.SectionName)
           .Get<RabbitMqConfiguration>();

        services.AddRebus(
                   conf => conf
                       .Transport(trans => trans.UseRabbitMq(rmqConfiguration.GetConnection, rmqConfiguration.Queue))
                       .Serialization(x => x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
                                       .Options(opt =>
                                       {
                                           opt.RetryStrategy(rmqConfiguration.ErrorQueue, rmqConfiguration.MaxRetry, true);
                                           opt.Decorate<ISerializer>(serializer =>
                                                new MessageDeserializer(serializer.Get<ISerializer>()));
                                       }),
                                   onCreated: async bus =>
                   {
                       await bus.Advanced.Topics.Subscribe(nameof(AccountCreatedEvent)).ConfigureAwait(false);                       
                   });

        services.AutoRegisterHandlersFromAssemblyOf<AccountCreatedEventHandler>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();