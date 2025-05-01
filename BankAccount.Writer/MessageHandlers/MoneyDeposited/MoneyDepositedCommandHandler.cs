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
        // here the AccountCreatedCommand is an integration Command, an instruction from outside your domain, you shouldn't persist directly as an event        
        var account = await _accountRepository.GetAsync(message.Email).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.Email}' not found!");
        }

        account.Deposit(message.Amount);
        await _accountRepository.AddAsync(account).ConfigureAwait(false);
    }
}
