using BankAccount.Writer.DomainEvents;
using BankAccount.Writer.Repositories;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountCreated;

public class AccountCreatedCommandHandler : IHandleMessages<AccountCreatedCommand>
{
    private readonly AccountUnitOfWork _unitOfWork;
    private AccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private OutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public AccountCreatedCommandHandler(AccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AccountCreatedCommand message)
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

    private async Task CreateAccountAsync(AccountCreatedCommand message)
    {
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account != null)
        {
            throw new InvalidOperationException($"Account with email '{message.AccountId}' already exists.");
        }

        account = new Account(message.AccountId);

        await OutboxEventRepository.SaveAsync(account.GetUncommittedEvents).ConfigureAwait(false);
        await AccountRepository.SaveAsync(account).ConfigureAwait(false);
    }
}
