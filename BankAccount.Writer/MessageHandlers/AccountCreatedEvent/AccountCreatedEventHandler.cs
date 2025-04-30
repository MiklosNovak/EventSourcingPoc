using Rebus.Bus;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreatedEvent;

public class AccountCreatedEventHandler : IHandleMessages<AccountCreatedEvent>
{    
    private readonly IBus _bus;
    private readonly ILogger<AccountCreatedEventHandler> _logger;    

    public AccountCreatedEventHandler(IBus bus, ILogger<AccountCreatedEventHandler> logger)
    {        
        _bus = bus;
        _logger = logger;        
    }
    
    public async Task Handle(AccountCreatedEvent message)
    {

    }
}
