namespace BankAccount.Writer.MessagePublishers;

public interface IMessagePublisher
{
    Task PublishMessagesAsync();
}