using BankAccount.Reader.AccountLogic;

namespace BankAccount.Reader.Repositories;

public interface IAccountRepository
{
    Task<Account> GetAsync(string accountId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(string accountId);
}