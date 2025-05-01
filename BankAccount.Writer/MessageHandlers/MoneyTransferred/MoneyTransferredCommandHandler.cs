using BankAccount.Writer.DomainEvents;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.MoneyTransferred;

public class MoneyTransferredCommandHandler : IHandleMessages<MoneyTransferredCommand>
{         
    private readonly AccountUnitOfWork _unitOfWork;
    
    public MoneyTransferredCommandHandler(AccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoneyTransferredCommand message)
    {
        try
        {
            await TransferMoney(message).ConfigureAwait(false);

            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await _unitOfWork.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task TransferMoney(MoneyTransferredCommand message)
    {
        var repository = _unitOfWork.AccountRepository;

        var account = await repository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.AccountId}' not found!");
        }

        var targetAccount = await repository.GetAsync(message.TargetAccountId).ConfigureAwait(false);

        if (targetAccount == null)
        {
            throw new InvalidOperationException($"Target account '{message.TargetAccountId}' not found!");
        }

        account.Withdrawn(message.Amount);
        targetAccount.Deposit(message.Amount);

        await repository.SaveAsync(account).ConfigureAwait(false);
        await repository.SaveAsync(targetAccount).ConfigureAwait(false);
    }
}
