using BankAccount.Writer.Configuration;
using BankAccount.Writer.MessageHandlers;
using BankAccount.Writer.MessageHandlers.MoneyDeposited;
using BankAccount.Writer.MessageHandlers.MoneyTransferred;
using BankAccount.Writer.MessageHandlers.MoneyWithdrawn;
using BankAccount.Writer.MessageHandlers.UserCreated;
using BankAccount.Writer.MessagePublishers;
using BankAccount.Writer.Repositories;
using BankAccount.Writer.UnitOfWork;
using Microsoft.Data.SqlClient;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Serialization.Json;

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
                                       await bus.Advanced.Topics.Subscribe(nameof(UserCreatedEvent)).ConfigureAwait(false);
                                       await bus.Advanced.Topics.Subscribe(nameof(MoneyDepositedEvent)).ConfigureAwait(false);
                                       await bus.Advanced.Topics.Subscribe(nameof(MoneyWithdrawnEvent)).ConfigureAwait(false);
                                       await bus.Advanced.Topics.Subscribe(nameof(MoneyTransferredEvent)).ConfigureAwait(false);
                                   });

        services.AddScoped(_ =>
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
