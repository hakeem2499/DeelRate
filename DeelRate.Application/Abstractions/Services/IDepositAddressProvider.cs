using DeelRate.Domain.Enums;
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Application.Abstractions.Services;

public interface IDepositAddressProvider
{
    DestinationAddress GetDepositAddressAsync(
        ExchangeOrderType exchangeOrderType,
        AddressType addressType,
        CryptoType? cryptoType,
        FiatType? fiatType
    );
}
