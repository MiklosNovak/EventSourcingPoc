using Rebus.Handlers;

namespace BankAccount.Reader.MessageHandlers.AccountCredited;

public class AccountCreditedEventHandler : IHandleMessages<AccountCreditedEvent>
{
    public async Task Handle(AccountCreditedEvent message)
    {
    }
}
