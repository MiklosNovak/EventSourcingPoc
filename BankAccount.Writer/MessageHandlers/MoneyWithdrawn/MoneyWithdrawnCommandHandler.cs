using BankAccount.Writer.MessageHandlers.MoneyWithdrawn;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class MoneyWithdrawnCommandHandler : IHandleMessages<MoneyWithdrawnCommand>
{         
    private readonly AccountRepository _accountRepository;

    public MoneyWithdrawnCommandHandler(AccountRepository accountRepository)
    {                
        _accountRepository = accountRepository;
    }

    public async Task Handle(MoneyWithdrawnCommand message)
    {        
        var account = await _accountRepository.GetAsync(message.Email).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.Email}' not found!");
        }

        account.Withdrawn(message.Amount);
        await _accountRepository.AddAsync(account).ConfigureAwait(false);
    }
}
