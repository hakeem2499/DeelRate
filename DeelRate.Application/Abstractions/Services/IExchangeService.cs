// Purpose: Interface for the ExchangeRateService class.
using DeelRate.Domain.Enums;
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Application.Abstractions.Services;

public interface IExchangeService
{
    Task<FiatAmount> CalculateFiatAmount(CryptoAmount cryptoAmount);
    Task<CryptoAmount> CalculateCryptoAmount(FiatAmount fiatAmount, CryptoType cryptoCurrency);
}
