using BankAccount.Writer.Repositories;
using Microsoft.Data.SqlClient;
using Rebus.Bus;
using Dapper;
using Newtonsoft.Json.Linq;
using Rebus.Messages;

public class MessagePublisher : BackgroundService
{
    private readonly SqlConnection _connection;
    private readonly ILogger<MessagePublisher> _logger;
    private readonly IBus _bus;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public MessagePublisher(SqlConnection connection, IBus bus, ILogger<MessagePublisher> logger)
    {
        _connection = connection;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var unProcessedEvents = await GetUnProcessedEventsAsync().ConfigureAwait(false);

                foreach (var unProcessedEvent in unProcessedEvents)
                {
                    try
                    {
                        await SendMessageAsync(unProcessedEvent).ConfigureAwait(false);
                        await MarkMessageAsProcessedAsync(unProcessedEvent).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Failed to publish outbox event: {unProcessedEvent.EventType}, version: {unProcessedEvent.Version}";
                        _logger.LogError(exception: ex, message: errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during outbox processing.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
   
    private async Task SendMessageAsync(OutboxEventEntity outbox)
    {
        // integration event what is published can be totally different, this is done for simplicity        
        var headers = new Dictionary<string, string> { { Headers.Type, outbox.EventType } };
        var payload = JObject.Parse(outbox.Data);
        payload["Version"] = outbox.Version;

        await _bus.Advanced.Topics.Publish(outbox.EventType, payload, headers).ConfigureAwait(false);        
    }   
    private async Task<IEnumerable<OutboxEventEntity>> GetUnProcessedEventsAsync()
    {
        var unProcessedSql = "SELECT TOP 10 * FROM dbo.OutboxEvents WHERE Published = 0";
        return await _connection.QueryAsync<OutboxEventEntity>(unProcessedSql).ConfigureAwait(false);        
    }

    private async Task MarkMessageAsProcessedAsync(OutboxEventEntity outbox)
    {
        var updateProcessedSql = "UPDATE dbo.OutboxEvents SET Published = 1 WHERE Version = @Version AND EventType = @EventType";
        await _connection.ExecuteAsync(updateProcessedSql, new { outbox.Version, outbox.EventType });
    }
}
