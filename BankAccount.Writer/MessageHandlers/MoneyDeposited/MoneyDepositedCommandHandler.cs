using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class MoneyDepositedCommandHandler : IHandleMessages<MoneyDepositedCommand>
{
    private readonly AccountUnitOfWork _unitOfWork;
    private AccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private OutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public MoneyDepositedCommandHandler(AccountUnitOfWork unitOfWork)
    {                
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoneyDepositedCommand message)
    {
        try
        {
            await DepositMoneyAsync(message).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await _unitOfWork.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task DepositMoneyAsync(MoneyDepositedCommand message)
    {
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.AccountId}' not found!");
        }

        account.Deposit(message.Amount);
        
        await OutboxEventRepository.SaveAsync(account.GetUncommittedEvents).ConfigureAwait(false);
        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
    }
}
