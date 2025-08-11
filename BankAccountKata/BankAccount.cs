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
        EnsureNoMoreThanThreeWithdrawalsToday();
        EnsureIsPositive(money);
        EnsureBalanceIsSufficient(money);
        HandleTransaction(-money);
    }

    private void EnsureNoMoreThanThreeWithdrawalsToday()
    {
        var withdrawalCount = GetTodaysWithdrawalCount();
        ThrowIfMoreThanThreeWithdrawals(withdrawalCount);
    }

    private static void ThrowIfMoreThanThreeWithdrawals(int withdrawalCount)
    {
        if (withdrawalCount >= 3)
            throw new InvalidOperationException("Cannot exceed 3 withdrawals per day.");
    }

    private int GetTodaysWithdrawalCount()
    {
        return transactions.Count(IsTodaysWithdrawal);
    }

    private bool IsTodaysWithdrawal(Transaction transaction)
    {
        return transaction.Amount < 0 && transaction.Date.Equals(dateProvider.Today);
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