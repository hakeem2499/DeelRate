using DeelRate.Common;

namespace DeelRate.Domain.Enums;

public class CryptoAmount : ValueObject
{
    public decimal Value { get; private set; }
    public CryptoType CryptoCurrency { get; private set; }

    private CryptoAmount() { }

    public CryptoAmount(decimal value, CryptoType cryptoCurrency)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Value must be greater than 0.", nameof(value));
        }
        if (cryptoCurrency == default)
        {
            throw new ArgumentException(
                "Crypto currency must be specified.",
                nameof(cryptoCurrency)
            );
        }

        Value = value;
        CryptoCurrency = cryptoCurrency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CryptoCurrency;
    }
}
