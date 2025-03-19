// Purpose: Implementation of the IExchangeRateService interface for fetching exchange rates from the CoinAPI service.
using DeelRate.Application.Abstractions.Services;
using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;
using DeelRate.Infrastructure.Services.CoinApiClient;
using Microsoft.Extensions.Caching.Memory;
using Refit;

namespace DeelRate.Infrastructure.Services.Exchange;

public class ExchangeRateService : IExchangeRateService
{
    private readonly ICoinApi _coinApiService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(1); // Adjust based on needs

    public ExchangeRateService(ICoinApi coinApiService, IMemoryCache cache)
    {
        _coinApiService = coinApiService ?? throw new ArgumentNullException(nameof(coinApiService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Result<ExchangeRate>> GetCurrencyPairExchangeRateAsync(
        CurrencyPair currencyPair
    )
    {
        if (currencyPair == null)
        {
            return Result.Failure<ExchangeRate>(
                Error.Validation("CurrencyPair.Required", "Currency pair is required.")
            );
        }

        string cacheKey = $"ExchangeRate_{currencyPair.BaseCurrency}_{currencyPair.QuoteCurrency}";

        // Try to get the exchange rate from the cache
        if (_cache.TryGetValue(cacheKey, out ExchangeRate? cachedRate))
        {
            return cachedRate != null
                ? Result.Success(cachedRate)
                : Result.Failure<ExchangeRate>(
                    Error.Failure("Cache.Error", "Cached rate is null.")
                );
        }

        try
        {
            // Fetch the exchange rate from the API
            CoinApiResponse? response = await _coinApiService.GetCoinExchangeRateAsync(
                currencyPair.BaseCurrency.ToString(),
                currencyPair.QuoteCurrency.ToString()
            );

            if (response == null)
            {
                return Result.Failure<ExchangeRate>(
                    Error.NotFound(
                        "Api.Error",
                        $"No exchange rate found for {currencyPair.BaseCurrency}/{currencyPair.QuoteCurrency}."
                    )
                );
            }

            // Parse the currency types
            if (
                !Enum.TryParse<CryptoType>(response.AssetIdBase, out CryptoType baseCurrency)
                || !Enum.TryParse<CryptoType>(response.AssetIdQuote, out CryptoType quoteCurrency)
            )
            {
                return Result.Failure<ExchangeRate>(
                    Error.Validation(
                        "Api.Error",
                        $"Invalid currency type in response for {currencyPair.BaseCurrency}/{currencyPair.QuoteCurrency}."
                    )
                );
            }

            // Create the exchange rate
            Result<ExchangeRate> exchangeRateResult = ExchangeRate.Create(
                new CurrencyPair(baseCurrency.ToString(), quoteCurrency.ToString()),
                response.Rate,
                response.Time
            );

            if (exchangeRateResult.IsFailure)
            {
                return exchangeRateResult;
            }

            // Cache the exchange rate
            _cache.Set(cacheKey, exchangeRateResult.Value, _cacheDuration);

            return Result.Success(exchangeRateResult.Value);
        }
        catch (ApiException ex)
        {
            return Result.Failure<ExchangeRate>(
                Error.Failure(
                    "Api.Error",
                    $"Failed to fetch rate for {currencyPair.BaseCurrency}/{currencyPair.QuoteCurrency}: {ex.Message}"
                )
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<ExchangeRate>(
                Error.Failure(
                    "Api.Error",
                    $"Unexpected error fetching rate for {currencyPair.BaseCurrency}/{currencyPair.QuoteCurrency}: {ex.Message}"
                )
            );
        }
    }

    public async Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync(
        IEnumerable<CurrencyPair> currencyPairs
    )
    {
        if (currencyPairs == null || !currencyPairs.Any())
        {
            return Result.Failure<List<ExchangeRate>>(
                Error.Validation(
                    "CurrencyPairs.Required",
                    "At least one currency pair is required."
                )
            );
        }

        var exchangeRates = new List<ExchangeRate>();
        var errors = new List<Error>();

        IEnumerable<Task<Result<ExchangeRate>>> tasks = currencyPairs.Select(async pair =>
        {
            string cacheKey = $"ExchangeRate_{pair.BaseCurrency}_{pair.QuoteCurrency}";

            // Try to get the exchange rate from the cache
            if (_cache.TryGetValue(cacheKey, out ExchangeRate? cachedRate))
            {
                return cachedRate != null
                    ? Result.Success(cachedRate)
                    : Result.Failure<ExchangeRate>(
                        Error.Failure("Cache.Error", "Cached rate is null.")
                    );
            }

            try
            {
                // Fetch the exchange rate from the API
                CoinApiResponse? response = await _coinApiService.GetCoinExchangeRateAsync(
                    pair.BaseCurrency.ToString(),
                    pair.QuoteCurrency.ToString()
                );

                if (response == null)
                {
                    return Result.Failure<ExchangeRate>(
                        Error.NotFound(
                            "Api.Error",
                            $"No exchange rate found for {pair.BaseCurrency}/{pair.QuoteCurrency}."
                        )
                    );
                }

                // Parse the currency types
                if (
                    !Enum.TryParse<CryptoType>(response.AssetIdBase, out CryptoType baseCurrency)
                    || !Enum.TryParse<CryptoType>(
                        response.AssetIdQuote,
                        out CryptoType quoteCurrency
                    )
                )
                {
                    return Result.Failure<ExchangeRate>(
                        Error.Validation(
                            "Api.Error",
                            $"Invalid currency type in response for {pair.BaseCurrency}/{pair.QuoteCurrency}."
                        )
                    );
                }

                // Create the exchange rate
                Result<ExchangeRate> exchangeRateResult = ExchangeRate.Create(
                    new CurrencyPair(baseCurrency.ToString(), quoteCurrency.ToString()),
                    response.Rate,
                    response.Time
                );

                if (exchangeRateResult.IsFailure)
                {
                    return exchangeRateResult;
                }

                // Cache the exchange rate
                _cache.Set(cacheKey, exchangeRateResult.Value, _cacheDuration);

                return Result.Success(exchangeRateResult.Value);
            }
            catch (ApiException ex)
            {
                return Result.Failure<ExchangeRate>(
                    Error.Failure(
                        "Api.Error",
                        $"Failed to fetch rate for {pair.BaseCurrency}/{pair.QuoteCurrency}: {ex.Message}"
                    )
                );
            }
            catch (Exception ex)
            {
                return Result.Failure<ExchangeRate>(
                    Error.Failure(
                        "Api.Error",
                        $"Unexpected error fetching rate for {pair.BaseCurrency}/{pair.QuoteCurrency}: {ex.Message}"
                    )
                );
            }
        });

        // Wait for all tasks to complete
        Result<ExchangeRate>[] results = await Task.WhenAll(tasks);

        // Process results
        foreach (Result<ExchangeRate> result in results)
        {
            if (result.IsSuccess)
            {
                exchangeRates.Add(result.Value);
            }
            else
            {
                errors.Add(result.Error);
            }
        }

        // Return partial results if some exchange rates were retrieved successfully
        if (exchangeRates.Count > 0)
        {
            return Result.Success(exchangeRates);
        }

        // If no exchange rates were retrieved, return the first error
        return Result.Failure<List<ExchangeRate>>(
            errors.FirstOrDefault()
                ?? Error.Failure("Api.Error", "Failed to retrieve any exchange rates.")
        );
    }

    public async Task<Result<List<ExchangeRate>>> GetExchangeRatesByBaseCurrencyAsync(
        CryptoType baseCurrency
    )
    {
        IEnumerable<CurrencyPair> supportedQuotesCurrencies = Enum.GetValues<CryptoType>()
            .Where(c => c != baseCurrency)
            .Select(c => new CurrencyPair(baseCurrency.ToString(), c.ToString()));
        return await GetExchangeRatesAsync(supportedQuotesCurrencies);
    }

    public async Task<Result<List<CurrencyPair>>> GetSupportedCurrencyPairsAsync() 
        {
            
        }
}
