using Rebus.Messages;
using Rebus.Pipeline;

namespace BankAccount.Reader.MessageHandlers;

public class RebusErrorHandlingStep : IIncomingStep
{    
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        try
        {            
            await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {            
            var message = context.Load<TransportMessage>();
            message.Headers["ExceptionMessage"] = ex.Message;
            throw;
        }
    }
}
