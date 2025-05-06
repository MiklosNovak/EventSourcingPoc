using BankAccount.Writer.AccountLogic;

namespace BankAccount.Writer.Repositories;

public interface IAccountRepository
{
    Task<Account> GetAsync(string email);
    Task SaveAsync(Account account);
}