using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.MoneyDeposited;

public class MoneyDepositedHandler : IHandleMessages<MoneyDepositedEvent>
{
    private readonly IAccountUnitOfWork _unitOfWork;
    private IAccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private IOutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public MoneyDepositedHandler(IAccountUnitOfWork unitOfWork)
    {                
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MoneyDepositedEvent message)
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

    private async Task DepositMoneyAsync(MoneyDepositedEvent message)
    {
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            throw new InvalidOperationException($"Account '{message.AccountId}' not found!");
        }

        account.Deposit(message.Amount);
        
        await OutboxEventRepository.AddAsync(account.GetUncommittedEvents).ConfigureAwait(false);
        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
    }
}
