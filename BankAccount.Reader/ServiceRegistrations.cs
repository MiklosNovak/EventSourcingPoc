using BankAccount.Reader.Configuration;
using BankAccount.Reader.MessageHandlers;
using BankAccount.Reader.MessageHandlers.AccountCreated;
using BankAccount.Reader.MessageHandlers.AccountCredited;
using BankAccount.Reader.MessageHandlers.AccountDebited;
using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;
using BankAccount.Reader.MessagePublishers;
using BankAccount.Reader.MessageReplay;
using BankAccount.Reader.MongoDb;
using BankAccount.Reader.Repositories;
using MongoDB.Driver;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Serialization.Json;

namespace BankAccount.Reader;

public class ServiceRegistrations
{
    public static void RegisterServices(HostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;

        var rmqConfiguration = configuration
            .GetRequiredSection(RabbitMqConfiguration.SectionName)
            .Get<RabbitMqConfiguration>();

        var mongoDbConfiguration = configuration
            .GetRequiredSection(MongoDbConfiguration.SectionName)
            .Get<MongoDbConfiguration>();

        services.AddRebus(
            conf => conf
                .Transport(trans => trans.UseRabbitMq(rmqConfiguration!.GetConnection, rmqConfiguration.Queue))
                .Serialization(x => x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
                .Options(opt =>
                {
                    opt.RetryStrategy(rmqConfiguration!.ErrorQueue, rmqConfiguration.MaxRetry);
                    opt.Decorate<ISerializer>(serializer =>
                        new MessageDeserializer(serializer.Get<ISerializer>()));

                    opt.Decorate<IPipeline>(ctx =>
                    {
                        var pipeline = ctx.Get<IPipeline>();
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
                await bus.Advanced.Topics.Subscribe(nameof(AccountCreatedEvent)).ConfigureAwait(false);
                await bus.Advanced.Topics.Subscribe(nameof(AccountCreditedEvent)).ConfigureAwait(false);
                await bus.Advanced.Topics.Subscribe(nameof(AccountDebitedEvent)).ConfigureAwait(false);
                await bus.Advanced.Topics.Subscribe(nameof(AccountStateCorruptedEvent)).ConfigureAwait(false);
            });

        services.AutoRegisterHandlersFromAssemblyOf<AccountCreatedEvent>();

        services.Configure<MongoDbConfiguration>(configuration.GetSection(MongoDbConfiguration.SectionName));
        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddScoped<IMongoClient>(_ =>
        {
            var mongoUrl = new MongoUrl(mongoDbConfiguration.GetConnection);
            return new MongoClient(mongoUrl);
        });

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddSingleton<IMessageBuffer, MessageBuffer>();
        services.AddScoped<IMessagePublisher, MessagePublisher>();
        services.AddHostedService<MessageReplayer>();
    }
}
