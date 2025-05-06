using BankAccount.Writer.AccountLogic;

namespace BankAccount.Writer.Repositories.Accounts;

public interface IAccountRepository
{
    Task<Account> GetAsync(string email);
    Task SaveAsync(Account account);
}