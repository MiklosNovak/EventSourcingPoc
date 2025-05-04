namespace BankAccount.Reader.AccountLogic;

public class Account
{    
    public string AccountId { get; private set; }  
    
    public decimal Balance { get; private set; }

    public int Version { get; private set; }

    public int OldVersion { get; private set; }

    public Account(string email, decimal balance, int version)
    {
        AccountId = email;
        Balance = balance;
        Version = version;
        OldVersion = version;
    }

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }

    public void Withdrawn(decimal amount)
    {
       Balance -= amount;
    }

    public void SetVersion(int version)
    {
        Version = version;
    }
}
