namespace BankAccount.Writer.MessagePublishers;

public class MessagePublisherBackgroundService : BackgroundService
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<MessagePublisherBackgroundService> _logger;

    public MessagePublisherBackgroundService(IMessagePublisher messagePublisher, ILogger<MessagePublisherBackgroundService> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _messagePublisher.PublishMessagesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during outbox processing.");
            }

            var delay = TimeSpan.FromSeconds(5);
            await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
        }
    }
}