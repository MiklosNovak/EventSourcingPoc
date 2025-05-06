using BankAccount.Reader.MessagePublishers;
using BankAccount.Reader.Repositories;
using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountStateCorrupted;

public class AccountStateCorruptedEventHandler : IHandleMessages<AccountStateCorruptedEvent>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMessagePublisher _messagePublisher;

    public AccountStateCorruptedEventHandler(IAccountRepository accountRepository, IMessagePublisher messagePublisher)
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
