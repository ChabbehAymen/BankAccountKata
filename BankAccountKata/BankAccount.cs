namespace BankAccountKata;

public class BankAccount
{
    readonly IDateProvider dateProvider;
    readonly Transactions transactions = new();


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
        EnsureWithdrawalRulesAreSatisfied(money);

        HandleTransaction(-money);
    }

    private void EnsureWithdrawalRulesAreSatisfied(int money)
    {
        EnsureIsPositive(money);

        EnsureBalanceIsSufficient(money);
        EnsureWithdrawalThresholdsAreNotExceeded(money);
    }

    private void EnsureWithdrawalThresholdsAreNotExceeded(int money)
    {
        EnsureDailyWithdrawalThresholdsAreNotExceeded(money);
    }

    private void EnsureDailyWithdrawalThresholdsAreNotExceeded(int money)
    {
        EnsureMaximumDailyWithdrawalAmountIsNotExceeded(money);
        EnsureMaximumDailyWithdrawalCountIsNotExceeded();
    }

    private void EnsureMaximumDailyWithdrawalAmountIsNotExceeded(int requestedAmount)
    {
        var candidateTotalAmount = AddRequestedAmountToAmountWithdrewToday(requestedAmount);
        ThrowIfDailyWithdrawalThresholdIsExceeded(candidateTotalAmount);
    }

    private int AddRequestedAmountToAmountWithdrewToday(int requestedAmount)
    {
        var todaysAmount = GetAmountWithdrewToday();
        var candidateTodaysAmount = todaysAmount + requestedAmount;
        return candidateTodaysAmount;
    }

    private static void ThrowIfDailyWithdrawalThresholdIsExceeded(int candidateTodaysAmount)
    {
        const int MaximumDailyWithdrawalAmount = 100;
        if (candidateTodaysAmount > MaximumDailyWithdrawalAmount)
            throw new InvalidOperationException($"Cannot withdraw more than {MaximumDailyWithdrawalAmount} per day.");
    }

    private int GetAmountWithdrewToday()
    {
        var todaysNegativeWithdrawalSum = transactions.Where(IsWithdrewToday).Sum(t => t.Amount);
        var todaysPositiveWithdrawalAmount = Math.Abs(todaysNegativeWithdrawalSum);

        return todaysPositiveWithdrawalAmount;
    }

    private void EnsureMaximumDailyWithdrawalCountIsNotExceeded()
    {
        var todaysWithdrawalCount = GetTodaysWithdrawalCount();
        ThrowIfDailyWithdrawalCountThresholdIsExceeded(todaysWithdrawalCount);
    }

    private int GetTodaysWithdrawalCount()
    {
        return transactions.Count(IsWithdrewToday);
    }

    private static void ThrowIfDailyWithdrawalCountThresholdIsExceeded(int todaysWithdrawalCount)
    {
        const int MaximumDailyWithdrawalCount = 3;
        if (todaysWithdrawalCount == MaximumDailyWithdrawalCount)
            throw new InvalidOperationException($"Cannot exceed {MaximumDailyWithdrawalCount} withdrawals per day.");
    }

    private bool IsWithdrewToday(Transaction transaction)
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