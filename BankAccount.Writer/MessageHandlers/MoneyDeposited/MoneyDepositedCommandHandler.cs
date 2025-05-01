using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class MoneyDepositedCommandHandler : IHandleMessages<MoneyDepositedCommand>
{         
    private readonly AccountRepository _accountRepository;

    public MoneyDepositedCommandHandler(AccountRepository accountRepository)
    {                
        _accountRepository = accountRepository;
    }

    public async Task Handle(MoneyDepositedCommand message)
    {        
        var account = await _accountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.AccountId}' not found!");
        }

        account.Deposit(message.Amount);
        await _accountRepository.SaveAsync(account).ConfigureAwait(false);
    }
}
