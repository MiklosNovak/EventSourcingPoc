using BankAccount.Reader.Repositories;
using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountCredited;

public class AccountCreditedEventHandler : IHandleMessages<AccountCreditedEvent>
{
    private readonly AccountRepository _accountRepository;
    private readonly ILogger<AccountCreditedEventHandler> _logger;

    public AccountCreditedEventHandler(AccountRepository accountRepository, ILogger<AccountCreditedEventHandler> logger)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }
    public async Task Handle(AccountCreditedEvent message)
    {
        var account = await _accountRepository.GetAsync(message.AccountId).ConfigureAwait(false);
        if (account == null)
        {
            _logger.LogWarning("Account {MessageAccountId} does not exist. Ignoring event.", message.AccountId);
            return;
        }

        if (account.Version >= message.Version)
        {
            _logger.LogWarning("Event version {MessageVersion} is not greater than current version {AccountVersion}. Ignoring event.", message.Version, account.Version);
            return;
        }

        account.Deposit(message.Amount);
        account.SetVersion(message.Version);
        await _accountRepository.UpdateAsync(account).ConfigureAwait(false);
    }
}
