using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.MoneyTransferred;

public class MoneyTransferredHandler : IHandleMessages<MoneyTransferredEvent>
{         
    private readonly AccountUnitOfWork _unitOfWork;
    private AccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private OutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public MoneyTransferredHandler(AccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoneyTransferredEvent message)
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

    public async Task TransferMoney(MoneyTransferredEvent message)
    {
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.AccountId}' not found!");
        }

        var targetAccount = await AccountRepository.GetAsync(message.TargetAccountId).ConfigureAwait(false);

        if (targetAccount == null)
        {
            throw new InvalidOperationException($"Target account '{message.TargetAccountId}' not found!");
        }

        account.Withdrawn(message.Amount);
        targetAccount.Deposit(message.Amount);

        var uncommittedEvents = account.GetUncommittedEvents.Concat(targetAccount.GetUncommittedEvents);
        await OutboxEventRepository.SaveAsync(uncommittedEvents).ConfigureAwait(false);        

        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
        await AccountRepository.SaveAsync(targetAccount).ConfigureAwait(false);
    }
}
