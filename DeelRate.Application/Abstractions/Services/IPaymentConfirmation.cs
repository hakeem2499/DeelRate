using DeelRate.Domain.Common;
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Application.Abstractions.Services;

public interface IPaymentConfirmation
{
    Task<Result> ConfirmCryptoPayment(DestinationAddress address, CryptoAmount expectedAmount);
    Task<Result> ConfirmFiatPayment(DestinationAddress address, FiatAmount expectedAmount);
}
