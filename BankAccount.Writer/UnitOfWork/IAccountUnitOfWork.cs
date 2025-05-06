using BankAccount.Writer.Repositories.Accounts;
using BankAccount.Writer.Repositories.OutboxEvents;

namespace BankAccount.Writer.UnitOfWork;

public interface IAccountUnitOfWork : IDisposable
{
    IAccountRepository AccountRepository { get; }
    IOutboxEventRepository OutboxEventRepository { get; }
    Task CommitAsync();
    Task RollbackAsync();
}