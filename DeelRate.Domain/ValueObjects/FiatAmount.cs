using DeelRate.Common;
using DeelRate.Domain.Enums;

namespace DeelRate.Domain.ValueObjects;

public class FiatAmount : ValueObject
{
    public FiatType FiatType { get; private set; }
    public decimal Amount { get; private set; }

    public FiatAmount(FiatType fiatType, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0.", nameof(amount));
        }
        if (fiatType == default)
        {
            throw new ArgumentException("Fiat type must be specified.", nameof(fiatType));
        }
        FiatType = fiatType;
        Amount = amount;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FiatType;
        yield return Amount;
    }
}
