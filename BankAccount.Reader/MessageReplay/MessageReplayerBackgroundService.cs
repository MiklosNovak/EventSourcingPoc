namespace BankAccount.Reader.MessageReplay;

public class MessageReplayerBackgroundService : BackgroundService
{
    private readonly MessageReplayer _messageReplayer;
    private readonly ILogger<MessageReplayerBackgroundService> _logger;

    public MessageReplayerBackgroundService(MessageReplayer messageReplayer, ILogger<MessageReplayerBackgroundService> logger)
    {
        _messageReplayer = messageReplayer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);

            try
            {
                await _messageReplayer.ReplayEventsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during buffer processing.");
            }
        }
    }
}