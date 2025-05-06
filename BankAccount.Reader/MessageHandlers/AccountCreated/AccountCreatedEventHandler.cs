using BankAccount.Reader.AccountLogic;
using BankAccount.Reader.Repositories;
using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountCreated;

public class AccountCreatedEventHandler : IHandleMessages<AccountCreatedEvent>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AccountCreatedEventHandler> _logger;

    public AccountCreatedEventHandler(IAccountRepository accountRepository, ILogger<AccountCreatedEventHandler> logger)
    {
        _logger = logger;
        _accountRepository = accountRepository;
    }

    public async Task Handle(AccountCreatedEvent message)
    {
        var account = await _accountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account != null)
        {
            _logger.LogWarning("Account {MessageAccountId} already exists. Ignoring event.", message.AccountId);
            return;
        }

        account = new Account(message.AccountId, 0, message.Version);
        await _accountRepository.AddAsync(account).ConfigureAwait(false);
    }
}
