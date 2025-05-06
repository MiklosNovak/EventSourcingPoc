using BankAccount.Writer.AccountLogic;
using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;
using BankAccount.Writer.UnitOfWork;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.UserCreated;

public class UserCreatedHandler : IHandleMessages<UserCreatedEvent>
{
    private readonly IAccountUnitOfWork _unitOfWork;
    private IAccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private IOutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public UserCreatedHandler(IAccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UserCreatedEvent message)
    {
        try
        {
            await CreateAccountAsync(message).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await _unitOfWork.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task CreateAccountAsync(UserCreatedEvent message)
    {
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account != null)
        {
            throw new InvalidOperationException($"Account with email '{message.AccountId}' already exists.");
        }

        account = new Account(message.AccountId);

        await OutboxEventRepository.AddAsync(account.GetUncommittedEvents).ConfigureAwait(false);
        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
    }
}
