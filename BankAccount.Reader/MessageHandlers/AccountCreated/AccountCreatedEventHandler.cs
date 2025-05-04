using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountCreated;

public class AccountCreatedEventHandler : IHandleMessages<AccountCreatedEvent>
{
    public async Task Handle(AccountCreatedEvent message)
    {
    }
}
