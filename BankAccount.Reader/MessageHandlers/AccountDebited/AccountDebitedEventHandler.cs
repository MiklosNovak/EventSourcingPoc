using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountDebited;

public class AccountDebitedEventHandler : IHandleMessages<AccountDebitedEvent>
{
    public async Task Handle(AccountDebitedEvent message)
    {
    }
}
