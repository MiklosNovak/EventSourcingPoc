using BankAccount.Writer.Configuration;
using BankAccount.Writer.Repositories;
using Microsoft.Data.SqlClient;
using Rebus.Config;
using BankAccount.Writer.MessageHandlers;
using Rebus.Pipeline.Receive;
using Rebus.Pipeline;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using BankAccount.Writer.MessageHandlers.MoneyWithdrawn;
using BankAccount.Writer.MessageHandlers.UserCreated;
using BankAccount.Writer.MessageHandlers.MoneyDeposited;
using BankAccount.Writer.MessageHandlers.MoneyTransferred;
using BankAccount.Writer.MessagePublishers;

namespace BankAccount.Writer;

public class ServiceRegistrations
{
    public static void RegisterServices(HostBuilderContext context, IServiceCollection services)
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
                       .Transport(trans => trans.UseRabbitMq(rmqConfiguration!.GetConnection, rmqConfiguration.Queue))
                       .Serialization(x => x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
                                       .Options(opt =>
                                       {
                                           opt.RetryStrategy(rmqConfiguration!.ErrorQueue, 5);
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
                                       await bus.Advanced.Topics.Subscribe(nameof(UserCreatedEvent)).ConfigureAwait(false);
                                       await bus.Advanced.Topics.Subscribe(nameof(MoneyDepositedEvent)).ConfigureAwait(false);
                                       await bus.Advanced.Topics.Subscribe(nameof(MoneyWithdrawnEvent)).ConfigureAwait(false);
                                       await bus.Advanced.Topics.Subscribe(nameof(MoneyTransferredEvent)).ConfigureAwait(false);
                                   });

        services.AddScoped(sp =>
        {
            var conn = new SqlConnection(msSqlConfiguration!.GetConnectionString);
            conn.Open();
            return conn;
        });

        services.AddScoped<AccountRepository>();
        services.AddScoped<OutboxEventEntity>();
        services.AddScoped<OutboxEventRepository>();
        services.AddScoped<AccountDomainEventDeserializer>();
        services.AddScoped<AccountUnitOfWork>();
        services.AddHostedService<MessagePublisher>();
        services.AutoRegisterHandlersFromAssemblyOf<UserCreatedHandler>();
    }
}
