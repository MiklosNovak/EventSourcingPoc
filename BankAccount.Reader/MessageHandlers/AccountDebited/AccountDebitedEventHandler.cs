using BankAccount.Reader.Repositories;
using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountDebited;

public class AccountDebitedEventHandler : IHandleMessages<AccountDebitedEvent>
{
    private readonly AccountRepository _accountRepository;
    private readonly ILogger<AccountDebitedEventHandler> _logger;

    public AccountDebitedEventHandler(AccountRepository accountRepository, ILogger<AccountDebitedEventHandler> logger)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }
    public async Task Handle(AccountDebitedEvent message)
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

        account.Withdrawn(message.Amount);
        account.SetVersion(message.Version);
        await _accountRepository.UpdateAsync(account).ConfigureAwait(false);
    }
}
