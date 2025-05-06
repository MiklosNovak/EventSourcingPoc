using BankAccount.Reader.MessageHandlers;
using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;

namespace BankAccount.Reader.MessagePublishers;

public interface IMessagePublisher
{
    Task PublishAccountReplyRequestedEventAsync(AccountStateCorruptedEvent message);

    Task SendLocalAsync(IIntegrationEvent message);
}