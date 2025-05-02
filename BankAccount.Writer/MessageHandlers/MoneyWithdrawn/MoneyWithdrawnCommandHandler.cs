using BankAccount.Writer.MessageHandlers.MoneyWithdrawn;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class MoneyWithdrawnCommandHandler : IHandleMessages<MoneyWithdrawnCommand>
{
    private readonly AccountUnitOfWork _unitOfWork;
    private AccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private OutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public MoneyWithdrawnCommandHandler(AccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoneyWithdrawnCommand message)
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

    private async Task WithdrawnAsync(MoneyWithdrawnCommand message)
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
