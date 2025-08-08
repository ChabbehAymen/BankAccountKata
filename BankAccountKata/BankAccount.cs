namespace BankAccountKata;

public class BankAccount
{
    readonly List<Transaction> transactions = [];
    readonly IDateProvider dateProvider;

    public BankAccount(IDateProvider dateProvider)
    {
        this.dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));
    }


    public void Deposit(int money)
    {
        HandleTransaction(money);
    }

    private void HandleTransaction(int money)
    {
        var transaction = CreateTransaction(money);
        SaveTransaction(transaction);
    }
    private Transaction CreateTransaction(int money)
    {
        var newBalance = GetBalance() + money;
        return new Transaction(money, dateProvider.Today, newBalance);
    }

    private void SaveTransaction(Transaction transaction)
    {
        transactions.Add(transaction);
    }

    public void Withdraw(int money)
    {
        HandleTransaction(-money);
    }

    public int GetBalance()
    {
        return SumAmountsOfAllTransactions();
    }

    private int SumAmountsOfAllTransactions()
    {
        return transactions.Sum(t => t.Amount);
    }

    public IEnumerable<Transaction> GetTransactions()
    {
        return transactions;
    }

}