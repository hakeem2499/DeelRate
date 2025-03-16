// Purpose: Interface for the ExchangeRateService class.
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Application.Abstractions.Services;

public interface IExchangeRateService
{
    Task<FiatAmount> CalculateFiatAmount(CryptoAmount cryptoAmount);
    Task<CryptoAmount> CalculateCryptoAmount(FiatAmount fiatAmount, string cryptoCurrency);
}
