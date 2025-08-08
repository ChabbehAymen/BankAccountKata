namespace BankAccountKata;

public class BankAccount
{
    readonly List<Transaction> transactions = [];
    readonly IDateProvider dateProvider;

    int balance = 0;

    public BankAccount(IDateProvider dateProvider)
    {
        this.dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));
    }


    public void Deposit(int money)
    {
        balance += money;
        SaveTransaction(money);
    }

    private void SaveTransaction(int money)
    {
        var transaction = new Transaction(money, dateProvider.Today, balance);
        transactions.Add(transaction);
    }

    public void Withdraw(int money)
    {
        balance -= money;
    }

    public int GetBalance()
    {
        return balance;
    }

    public IEnumerable<Transaction> GetTransactions()
    {
        return transactions;
    }

}