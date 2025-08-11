using System.Runtime.CompilerServices;
using System.Transactions;

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
        EnsureIsPositive(money);
        HandleTransaction(money);
    }

    private static void EnsureIsPositive(int money)
    {
        if (money < 0) 
            throw new ArgumentException("Negative amounts are not allowed.");
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
        EnsureIsPositive(money);
        EnsureBalanceIsSufficient(money);
        HandleTransaction(-money);
    }

    private void EnsureBalanceIsSufficient(int money)
    {
        if (money > GetBalance()) 
            throw new ArgumentException("Balance is insufficient.");
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