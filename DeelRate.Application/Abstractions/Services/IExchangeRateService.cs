using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;

namespace DeelRate.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync(IEnumerable<CurrencyPair> currencyPairs);
    Task<Result<ExchangeRate>> GetCurrencyPairExchangeRateAsync(CurrencyPair currencyPair);
    Task<Result<List<ExchangeRate>>> GetExchangeRatesByBaseCurrencyAsync(CryptoType baseCurrency);

    Task<Result<List<CurrencyPair>>> GetSupportedCurrencyPairsAsync();
}
