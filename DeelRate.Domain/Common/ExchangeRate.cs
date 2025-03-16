namespace DeelRate.Domain.Common;

public class ExchangeRate
{
    public CurrencyPair CurrencyPair { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime Timestamp { get; private set; }

    private ExchangeRate(CurrencyPair currencyPair, decimal rate, DateTime timestamp)
    {
        CurrencyPair = currencyPair;
        Rate = rate;
        Timestamp = timestamp;
    }

    public static Result<ExchangeRate> Create(
        CurrencyPair currencyPair,
        decimal rate,
        DateTime timestamp
    )
    {
        if (rate <= 0)
        {
            return Result.Failure<ExchangeRate>(
                Error.Conflict("Rate.Invalid", "Rate must be greater than 0.")
            );
        }

        return new ExchangeRate(currencyPair, rate, timestamp);
    }
}
