

namespace BankAccountKata;

public interface IDateProvider
{
    DateOnly Today { get; }
}