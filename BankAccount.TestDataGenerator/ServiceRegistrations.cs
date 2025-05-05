using BankAccount.TestDataGenerator.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Serialization.Json;

namespace BankAccount.TestDataGenerator;

public class ServiceRegistrations
{
    public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
    {
        var rmqConfiguration = configuration
           .GetRequiredSection(RabbitMqConfiguration.SectionName)
           .Get<RabbitMqConfiguration>();

        services.AddRebus(conf => conf
            .Transport(trans => trans.UseRabbitMqAsOneWayClient(rmqConfiguration!.GetConnection))
            .Serialization(x => x.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson)));

        services.AddScoped<MessagePublisher>();
    }
}
