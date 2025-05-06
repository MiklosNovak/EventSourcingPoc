using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.MoneyTransferred;

public class MoneyTransferredHandler : IHandleMessages<MoneyTransferredEvent>
{         
    private readonly IAccountUnitOfWork _unitOfWork;
    private IAccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private IOutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public MoneyTransferredHandler(IAccountUnitOfWork unitOfWork)
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

        var uncommittedEvents = account.GetUncommittedEvents.Concat(targetAccount.GetUncommittedEvents).ToList();
        await OutboxEventRepository.SaveAsync(uncommittedEvents).ConfigureAwait(false);        

        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
        await AccountRepository.SaveAsync(targetAccount).ConfigureAwait(false);
    }
}
