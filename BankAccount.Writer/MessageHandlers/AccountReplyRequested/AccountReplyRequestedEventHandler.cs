using BankAccount.Writer.Repositories;
using BankAccount.Writer.UnitOfWork;
using Rebus.Handlers;

namespace BankAccount.Writer.MessageHandlers.AccountReplyRequested;

public class AccountReplyRequestedEventHandler : IHandleMessages<AccountReplyRequestedEvent>
{
    private readonly AccountUnitOfWork _unitOfWork;

    private AccountRepository AccountRepository => _unitOfWork.AccountRepository;
    private OutboxEventRepository OutboxEventRepository => _unitOfWork.OutboxEventRepository;

    public AccountReplyRequestedEventHandler(AccountUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AccountReplyRequestedEvent message)
    {
        try
        {
            await ReplayEventsAsync(message).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);
        }
        catch
        {
            await _unitOfWork.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task ReplayEventsAsync(AccountReplyRequestedEvent message)
    {
        var account = await AccountRepository.GetAsync(message.AccountId).ConfigureAwait(false);

        if (account == null)
        {
            return;
        }

        await OutboxEventRepository.SaveAsync(account.GetVersionedDomainEvents).ConfigureAwait(false);
    }
}
