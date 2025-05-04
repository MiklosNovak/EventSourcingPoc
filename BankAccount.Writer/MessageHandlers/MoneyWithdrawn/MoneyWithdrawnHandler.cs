using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.MoneyWithdrawn;

public class MoneyWithdrawnHandler : IHandleMessages<MoneyWithdrawnEvent>
{
    private readonly AccountUnitOfWork _unitOfWork;
    private AccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private OutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public MoneyWithdrawnHandler(AccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoneyWithdrawnEvent message)
    {
        try
        {
            await WithdrawnAsync(message).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await _unitOfWork.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task WithdrawnAsync(MoneyWithdrawnEvent message)
    {    
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.AccountId}' not found!");
        }

        account.Withdrawn(message.Amount);

        await OutboxEventRepository.SaveAsync(account.GetUncommittedEvents).ConfigureAwait(false);
        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
    }
}
