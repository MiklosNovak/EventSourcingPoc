using BankAccount.Writer.Configuration;
using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.MessageHandlers;
using BankAccount.Writer.MessageHandlers.AccountCreated;
using BankAccount.Writer.Repositories;
using Microsoft.Data.SqlClient;
using Rebus.Config;
using Rebus.Pipeline.Receive;
using Rebus.Pipeline;
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

        var msSqlConfiguration = configuration
           .GetRequiredSection(MsSqlConfiguration.SectionName)
           .Get<MsSqlConfiguration>();        
        
        services.AddRebus(
                   conf => conf
                       .Transport(trans => trans.UseRabbitMq(rmqConfiguration.GetConnection, rmqConfiguration.Queue))
                       .Serialization(x => x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
                                       .Options(opt =>
                                       {
                                           opt.RetryStrategy(rmqConfiguration.ErrorQueue, 0);
                                           opt.Decorate<ISerializer>(serializer =>
                                                new MessageDeserializer(serializer.Get<ISerializer>()));

                                           opt.Decorate<IPipeline>(context =>
                                           {
                                               var pipeline = context.Get<IPipeline>();
                                               var errorHandlingStep = new RebusErrorHandlingStep(); // Add the custom error handling step
                                               return new PipelineStepInjector(pipeline)
                                                   .OnReceive(
                                                       errorHandlingStep,
                                                       PipelineRelativePosition.After,
                                                       typeof(DeserializeIncomingMessageStep));
                                           });
                                       }),
                                   onCreated: async bus =>
                   {
                       await bus.Advanced.Topics.Subscribe(nameof(AccountCreatedCommand)).ConfigureAwait(false);                       
                   });

        services.AddScoped<SqlConnection>(sp =>
        {            
            var conn = new SqlConnection(msSqlConfiguration.GetConnectionString);
            conn.Open();
            return conn;
        });

        services.AddScoped<AccountRepository>();
        services.AddScoped<AccountDomainEventDeserializer>();
        services.AutoRegisterHandlersFromAssemblyOf<AccountCreatedCommandHandler>();        
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();