using BankAccount.Reader.MessagePublishers;
using Rebus.Bus;
using BankAccount.Reader.Repositories;
using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountStateCorrupted;

public class AccountStateCorruptedEventHandler : IHandleMessages<AccountStateCorruptedEvent>
{
    private readonly AccountRepository _accountRepository;
    private readonly MessagePublisher _messagePublisher;

    public AccountStateCorruptedEventHandler(AccountRepository accountRepository, MessagePublisher messagePublisher)
    {
        _accountRepository = accountRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task Handle(AccountStateCorruptedEvent message)
    {
        await _accountRepository.DeleteAsync(message.AccountId).ConfigureAwait(false);
        await _messagePublisher.PublishAccountReplyRequestedEventAsync(message).ConfigureAwait(false);
    }
}
