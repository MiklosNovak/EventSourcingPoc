using BankAccount.Writer.Repositories;
using Microsoft.Data.SqlClient;

namespace BankAccount.Writer.UnitOfWork;

internal class AccountUnitOfWork : IAccountUnitOfWork
{
    private SqlTransaction _transaction;
    private bool _disposed;
    public IAccountRepository AccountRepository { get; }
    public IOutboxEventRepository OutboxEventRepository { get; }

    public AccountUnitOfWork(SqlConnection connection, IAccountDomainEventDeserializer deserializer)
    {
        _transaction = connection.BeginTransaction();
        AccountRepository = new AccountRepository(connection, deserializer, _transaction);
        OutboxEventRepository = new OutboxEventRepository(connection, _transaction);
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("Transaction already committed or rolled back.");

        await _transaction.CommitAsync().ConfigureAwait(false);
        await _transaction.DisposeAsync().ConfigureAwait(false);
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync().ConfigureAwait(false);
        await _transaction.DisposeAsync().ConfigureAwait(false);
        _transaction = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        _disposed = true;
    }
}