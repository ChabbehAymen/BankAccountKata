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
        SetBalanceTo(0);

        sut.Deposit(1);

        sut.GetBalance().Should().Be(1);
    }

    void SetBalanceTo(int money)
    {
        sut = CreateSut();
        sut.Deposit(money);
    }

    private BankAccount CreateSut()
    {
        return new(dateProvider);
    }

    [Fact]
    public void Balance_decreases_on_withdrawal()
    {
        SetBalanceTo(2);

        sut.Withdraw(1);

        sut.GetBalance().Should().Be(1);
    }

    [Fact]
    public void Transactions_are_empty_on_creation() 
    {
        var sut = CreateSut();

        sut.GetTransactions().Should().BeEmpty();
    }

    [Fact]
    public void Tracks_deposit_transactions()
    {
        GivenTodayIs(new(2015, 7, 1));

        sut.Deposit(1);

        var expected = new Transaction(Amount: 1, Date: new(2015, 7, 1), Balance: 1);
        sut.GetTransactions().Single().Should().Be(expected);
    }

    private void GivenTodayIs(DateOnly date)
    {
        ((DateProvider)dateProvider).Today = date;
    }

    [Fact]
    public void Tracks_withdrawals_transactions()
    {
        GivenTodayIs(new(2015, 7, 1));
        sut.Deposit(1);

        sut.Withdraw(1);

        var expected = new Transaction(Amount: -1, Date: new(2015, 7, 1), Balance: 0);
        sut.GetTransactions().Last().Should().Be(expected);
    }

    [Fact]
    public void Throws_when_withdrawing_more_than_balance() 
    {
        SetBalanceTo(1);

        var action = () => sut.Withdraw(2);

        action.Should().Throw<ArgumentException>()
                       .WithMessage("Balance is insufficient.");
    }
}

file class DateProvider : IDateProvider
{
    public DateOnly Today { get; set; } = new();
}