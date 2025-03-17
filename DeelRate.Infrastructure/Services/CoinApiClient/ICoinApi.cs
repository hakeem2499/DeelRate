// Purpose: Interface for CoinApi service.
using Refit;

namespace DeelRate.Infrastructure.Services.CoinApiClient;

public interface ICoinApi
{
    [Get("/v1/exchangerate/{assetIdBase}/{assetIdQuote}")]
    Task<CoinApiResponse?> GetCoinExchangeRateAsync(string assetIdBase, string assetIdQuote);
}
