using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class AccountCreatedEventHandler : IHandleMessages<AccountCreatedEvent>
{         
    private readonly AccountRepository _accountRepository;

    public AccountCreatedEventHandler(AccountRepository accountRepository)
    {                
        _accountRepository = accountRepository;
    }
    
    public async Task Handle(AccountCreatedEvent message)
    {
        var account = await _accountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account != null)
        {
            throw new InvalidOperationException($"Account with ID {message.AccountId} already exists.");            
        }
        
        account = new Account(message);        
        await _accountRepository.AddAsync(account).ConfigureAwait(false);
    }
}
