using DeelRate.Application.Abstractions.Services;
using DeelRate.Domain.Enums;
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Infrastructure.Services.Exchange;

public class ExchangeService : IExchangeService
{
    public Task<CryptoAmount> CalculateCryptoAmount(
        FiatAmount fiatAmount,
        CryptoType cryptoCurrency
    ) => throw new NotImplementedException();

    public Task<FiatAmount> CalculateFiatAmount(CryptoAmount cryptoAmount) =>
        throw new NotImplementedException();
}
