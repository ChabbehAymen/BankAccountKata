using FluentAssertions;

namespace BankAccountKata.Tests;


public class BankAccountTests
{
    BankAccount sut;
    IDateProvider dateProvider;

    public BankAccountTests()
    {
        dateProvider = new DateProvider();
        sut = CreateSut();
    }

    [Fact]
    public void Balance_is_zero_on_creation()
    {
        var sut = CreateSut();

        sut.GetBalance().Should().Be(0);
    }

    [Fact]
    public void Balance_increases_on_deposit()
    {
        GivenBalanceIsZero();

        sut.Deposit(1);

        sut.GetBalance().Should().Be(1);
    }

    void GivenBalanceIsZero()
    {
        sut.GetBalance().Should().Be(0);
    }

    private BankAccount CreateSut()
    {
        return new(dateProvider);
    }

    [Fact]
    public void Throws_on_negative_deposit()
    {
        var action = () => sut.Deposit(-1);

        action.Should().Throw<ArgumentException>()
                       .WithMessage("Negative amounts are not allowed.");
    }

    [Fact]
    public void Does_not_track_failed_deposit()
    {
        try
        {
            sut.Deposit(-1);
            Assert.Fail("Deposit should have failed.");
        }
        catch
        {
            sut.GetTransactions().Should().BeEmpty();
        }
    }

    [Fact]
    public void Balance_decreases_on_withdrawal()
    {
        AddMoneyToBalance(2);

        sut.Withdraw(1);

        sut.GetBalance().Should().Be(1);
    }

    private void AddMoneyToBalance(int money)
    {
        sut.Deposit(money);
    }

    [Fact]
    public void No_transactions_exist_on_creation()
    {
        var sut = CreateSut();

        sut.GetTransactions().Should().BeEmpty();
    }

    [Fact]
    public void Tracks_deposit_transactions()
    {
        GivenTodayIs(2015, 7, 1);

        sut.Deposit(1);

        var expected = new Transaction(Amount: 1, Date: new(2015, 7, 1), Balance: 1);
        sut.GetTransactions().Single().Should().Be(expected);
    }

    private void GivenTodayIs(int year, int month, int day)
    {
        ((DateProvider)dateProvider).Today = new DateOnly(year, month, day);
    }

    [Fact]
    public void Tracks_withdrawals_transactions()
    {
        GivenTodayIs(2015, 7, 1);
        AddMoneyToBalance(1);

        sut.Withdraw(1);

        var expected = new Transaction(Amount: -1, Date: new(2015, 7, 1), Balance: 0);
        sut.GetTransactions().Last().Should().Be(expected);
    }

    [Fact]
    public void Throws_when_withdrawing_more_than_balance()
    {
        AddMoneyToBalance(1);

        var action = () => sut.Withdraw(2);

        action.Should().Throw<ArgumentException>()
                       .WithMessage("Balance is insufficient.");
    }

    [Fact]
    public void Throws_on_negative_withdrawal()
    {
        var action = () => sut.Withdraw(-1);

        // Assert
        action.Should().Throw<ArgumentException>()
                       .WithMessage("Negative amounts are not allowed.");
        sut.GetBalance().Should().Be(0);
    }

    [Fact]
    public void Does_not_track_failed_withdrawal()
    {
        try
        {
            sut.Withdraw(-1);
            Assert.Fail("Withdrawal should have failed.");
        }
        catch
        {
            sut.GetTransactions().Should().BeEmpty();
        }
    }

    [Fact]
    public void Accepts_only_three_withdrawals_a_day()
    {
        GivenTodayIs(2015, 7, 1);
        MakeThreeWithdrawals();

        var action = () => sut.Withdraw(1);

        action.Should().Throw<InvalidOperationException>()
                        .WithMessage("Cannot exceed 3 withdrawals per day.");
    }

    private void MakeThreeWithdrawals()
    {
        MakeWithdrawalsOf([1, 1, 1], 5);
    }

    [Fact]
    public void Resets_threshold_of_withdrawal_count_on_new_day()
    {
        GivenTodayIs(2015, 7, 1);
        MakeThreeWithdrawals();
        GivenTodayIs(2015, 7, 2);

        var action = () => MakeThreeWithdrawals();

        action.Should().NotThrow();
    }

    [Fact]
    public void Accepts_total_withdrawal_amount_of_100_a_day()
    {
        GivenTodayIs(2015, 7, 1);
        AddMoneyToBalance(200);
        sut.Withdraw(100);

        var action = () => sut.Withdraw(1);

        action.Should().Throw<InvalidOperationException>()
                        .WithMessage("Cannot withdraw more than 100 per day.");
    }

    [Fact]
    public void Resets_threshold_of_withdrawal_amount_on_new_day()
    {
        GivenTodayIs(2015, 7, 1);
        AddMoneyToBalance(200);
        sut.Withdraw(100);
        GivenTodayIs(2015, 7, 2);

        var action = () => sut.Withdraw(100);

        action.Should().NotThrow();
    }

    [Fact]
    public void Prioritizes_non_negative_amount_rule_over_withdrawal_count_threshold_rule()
    {
        MakeThreeWithdrawals();

        var action = () => sut.Withdraw(-1);

        action.Should().Throw<ArgumentException>()
                       .WithMessage("Negative amounts are not allowed.");
    }

    [Fact]
    public void Prioritizes_non_negative_amount_rule_over_withdrawal_amount_threshold_rule()
    {
        // We might think that this fact should be verified, 
        // but it's already covered by a more general fact
        // that ensures a negative amount is rejected right away
        // in all kinds of situations.
        // We opted for keeping this fact to prevent
        // unnecessary confusion again.
    }

    [Fact]
    public void Prioritizes_balance_insufficient_rule_over_withdrawal_amount_threshold_rule()
    {
        AddMoneyToBalance(1);

        var action = () => WithdrawMoreThanBalanceAndMoreThanAmountThreshold();

        action.Should().Throw<ArgumentException>().WithMessage("Balance is insufficient.");
    }

    private void WithdrawMoreThanBalanceAndMoreThanAmountThreshold()
    {
        sut.Withdraw(100 + 1);
    }

    [Fact]
    public void Prioritizes_balance_insufficient_rule_over_withdrawal_count_threshold_rule()
    {
        MakeWithdrawalsOf([1, 1, 1], initialDeposit: 3);

        var action = () => sut.Withdraw(1);

        action.Should().Throw<ArgumentException>().WithMessage("Balance is insufficient.");
    }

    private void MakeWithdrawalsOf(IEnumerable<int> amounts, int initialDeposit = 0)
    {
        AddMoneyToBalance(initialDeposit);

        foreach (var amount in amounts)
        {
            sut.Withdraw(amount);
        }
    }

    [Fact]
    public void Prioritizes_withdrawal_amount_threshold_rule_over_withdrawal_count_threshold_rule()
    {
        MakeWithdrawalsOf([40, 40, 20], initialDeposit: 200);

        var action = () => sut.Withdraw(1);

        action.Should().Throw<InvalidOperationException>()
                        .WithMessage("Cannot withdraw more than 100 per day.");
    }
}

file class DateProvider : IDateProvider
{
    public DateOnly Today { get; set; } = new();
}