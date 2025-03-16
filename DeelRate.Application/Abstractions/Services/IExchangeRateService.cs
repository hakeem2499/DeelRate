using DeelRate.Domain.Common;

namespace DeelRate.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync(IEnumerable<CurrencyPair> currencyPairs);
}
