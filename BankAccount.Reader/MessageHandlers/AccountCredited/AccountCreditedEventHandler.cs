using BankAccount.Reader.MessageReplay;
using BankAccount.Reader.Repositories;
using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountCredited;

public class AccountCreditedEventHandler : IHandleMessages<AccountCreditedEvent>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountCreditedEventHandler> _logger;
    private readonly IMessageBuffer _messageBuffer;

    public AccountCreditedEventHandler(IAccountRepository accountRepository, ILogger<AccountCreditedEventHandler> logger, IMessageBuffer messageBuffer)
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _messageBuffer = messageBuffer;
    }
    public async Task Handle(AccountCreditedEvent message)
    {
        var account = await _accountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null || message.Version > account.Version + 1)
        {
            _messageBuffer.Add(message);
            return;
        }

        if (message.Version <= account.Version)
        {
            _logger.LogWarning("Message already processed. Ignoring event with version {MessageVersion} for account {AccountId}.", message.Version, message.AccountId);
            return;
        }

        account.Deposit(message.Amount);
        account.SetVersion(message.Version);
        await _accountRepository.UpdateAsync(account).ConfigureAwait(false);
    }
}
