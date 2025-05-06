using BankAccount.Reader.MessageHandlers.AccountStateCorrupted;
using BankAccount.Reader.MessageReplay;

namespace BankAccount.Reader.MessagePublishers;

public interface IMessagePublisher
{
    Task PublishAccountReplyRequestedEventAsync(AccountStateCorruptedEvent message);

    Task SendLocalAsync(ReplayableEvent message);
}