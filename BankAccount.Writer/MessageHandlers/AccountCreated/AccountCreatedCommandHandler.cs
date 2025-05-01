using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class AccountCreatedCommandHandler : IHandleMessages<AccountCreatedCommand>
{         
    private readonly AccountRepository _accountRepository;

    public AccountCreatedCommandHandler(AccountRepository accountRepository)
    {                
        _accountRepository = accountRepository;
    }

    public async Task Handle(AccountCreatedCommand message)
    {
        // here the AccountCreatedCommand is an integration Command, an instruction from outside your domain, you shouldn't persist directly as an event        
        var account = await _accountRepository.GetAsync(message.Email).ConfigureAwait(false);

        if (account != null)
        {
            throw new InvalidOperationException($"Account with email '{message.Email}' already exists.");
        }

        account = new Account(message.Email);
        await _accountRepository.AddAsync(account).ConfigureAwait(false);
    }
}
