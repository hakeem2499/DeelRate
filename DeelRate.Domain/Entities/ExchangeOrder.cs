using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Domain.Entities;

public class ExchangeOrder : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public CryptoType? CryptoType { get; private set; }
    public ExchangeOrderType ExchangeOrderType { get; private set; }
    public CryptoAmount? CryptoAmount { get; private set; }
    public FiatAmount? FiatAmount { get; private set; }
    public ExchangeOrderStatus Status { get; private set; }
    public DestinationAddress? UserDestinationAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public ExchangeCompleted? ExchangeCompleted { get; private set; }

    // Private constructor for EF Core or serialization
    private ExchangeOrder() { }

    // Factory method for initiating an exchange order
    public static Result<ExchangeOrder> Initiate(
        Guid userId,
        CryptoType cryptoType,
        ExchangeOrderType exchangeOrderType,
        CryptoAmount? cryptoAmount,
        FiatAmount? fiatAmount,
        DestinationAddress? userDestinationAddress
    )
    {
        // Guard clauses and validation
        if (userId == Guid.Empty)
        {
            return Result.Failure<ExchangeOrder>(
                Error.Validation("ExchangeOrder.InvalidUserId", "User ID must be a valid GUID.")
            );
        }

        if (!Enum.IsDefined(typeof(ExchangeOrderType), exchangeOrderType))
        {
            return Result.Failure<ExchangeOrder>(
                Error.Validation("ExchangeOrder.InvalidType", "Invalid exchange order type.")
            );
        }

        if (!Enum.IsDefined(typeof(CryptoType), cryptoType))
        {
            return Result.Failure<ExchangeOrder>(
                Error.Validation("ExchangeOrder.InvalidCryptoType", "Invalid cryptocurrency type.")
            );
        }

        // Validate amounts and destination based on order type
        if (exchangeOrderType == ExchangeOrderType.Buy)
        {
            if (fiatAmount is null)
            {
                return Result.Failure<ExchangeOrder>(
                    Error.Validation(
                        "ExchangeOrder.MissingFiatAmount",
                        "FiatAmount is required for a Buy order."
                    )
                );
            }

            if (cryptoAmount is not null)
            {
                return Result.Failure<ExchangeOrder>(
                    Error.Validation(
                        "ExchangeOrder.UnexpectedCryptoAmount",
                        "CryptoAmount should not be provided for a Buy order at initiation."
                    )
                );
            }

            if (userDestinationAddress?.DestinationAddressType != AddressType.CryptoDepositAddress)
            {
                return Result.Failure<ExchangeOrder>(
                    Error.Validation(
                        "ExchangeOrder.InvalidDestination",
                        "For a Buy order, destination must be a crypto address."
                    )
                );
            }
        }
        else if (exchangeOrderType is ExchangeOrderType.Sell)
        {
            if (cryptoAmount is null)
            {
                return Result.Failure<ExchangeOrder>(
                    Error.Validation(
                        "ExchangeOrder.MissingCryptoAmount",
                        "CryptoAmount is required for a Sell order."
                    )
                );
            }

            if (fiatAmount is not null)
            {
                return Result.Failure<ExchangeOrder>(
                    Error.Validation(
                        "ExchangeOrder.UnexpectedFiatAmount",
                        "FiatAmount should not be provided for a Sell order at initiation."
                    )
                );
            }

            if (userDestinationAddress?.DestinationAddressType != AddressType.FiatAccountNumber)
            {
                return Result.Failure<ExchangeOrder>(
                    Error.Validation(
                        "ExchangeOrder.InvalidDestination",
                        "For a Sell order, destination must be a fiat account."
                    )
                );
            }
        }

        if (userDestinationAddress is null)
        {
            return Result.Failure<ExchangeOrder>(
                Error.Validation(
                    "ExchangeOrder.MissingDestination",
                    "User destination address is required."
                )
            );
        }

        // Create the order
        var order = new ExchangeOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CryptoType = cryptoType,
            ExchangeOrderType = exchangeOrderType,
            CryptoAmount = cryptoAmount, // Null for Buy, populated for Sell
            FiatAmount = fiatAmount, // Populated for Buy, null for Sell
            UserDestinationAddress = userDestinationAddress,
            Status = ExchangeOrderStatus.Initiated,
            CreatedAt = DateTime.UtcNow,
        };

        return Result.Success(order);
    }

    // State transition methods
    public Result MarkPaymentPending()
    {
        if (Status != ExchangeOrderStatus.Initiated)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.NotInitiated",
                    "Order must be in Initiated state to mark payment as pending."
                )
            );
        }

        // Amounts are already validated at Initiate
        Status = ExchangeOrderStatus.PaymentPending;
        return Result.Success();
    }

    public Result ConfirmPayment(
        FiatAmount? actualFiatReceived,
        CryptoAmount? actualCryptoReceived,
        decimal rate
    )
    {
        if (Status != ExchangeOrderStatus.PaymentPending)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.NotPaymentPending",
                    "Order must be in PaymentPending state to confirm payment."
                )
            );
        }

        if (rate <= 0)
        {
            return Result.Failure(
                Error.Validation("ExchangeOrder.InvalidRate", "Exchange rate must be positive.")
            );
        }

        if (ExchangeOrderType == ExchangeOrderType.Buy)
        {
            if (actualFiatReceived is null)
            {
                return Result.Failure(
                    Error.Validation(
                        "ExchangeOrder.MissingFiatReceived",
                        "Actual fiat received is required for Buy confirmation."
                    )
                );
            }

            if (!AreFiatAmountsEqual(FiatAmount, actualFiatReceived))
            {
                return Result.Failure(
                    Error.Validation(
                        "ExchangeOrder.FiatMismatch",
                        "Actual fiat received does not match requested amount."
                    )
                );
            }

            CryptoAmount = new CryptoAmount(actualFiatReceived.Amount / rate, CryptoType.Value);
        }
        else if (ExchangeOrderType == ExchangeOrderType.Sell)
        {
            if (actualCryptoReceived is null)
            {
                return Result.Failure(
                    Error.Validation(
                        "ExchangeOrder.MissingCryptoReceived",
                        "Actual crypto received is required for Sell confirmation."
                    )
                );
            }

            if (!AreCryptoAmountsEqual(CryptoAmount, actualCryptoReceived))
            {
                return Result.Failure(
                    Error.Validation(
                        "ExchangeOrder.CryptoMismatch",
                        "Actual crypto received does not match expected amount."
                    )
                );
            }
            FiatType fiatType = actualFiatReceived.FiatType;
            FiatAmount = new FiatAmount(fiatType, actualCryptoReceived.Value * rate);
        }

        Status = ExchangeOrderStatus.ConfirmPayment;
        return Result.Success();
    }

    public Result<ExchangeCompleted> CompleteExchange()
    {
        if (Status != ExchangeOrderStatus.ConfirmPayment)
        {
            return Result.Failure<ExchangeCompleted>(
                Error.Validation(
                    "ExchangeOrder.NotConfirmed",
                    "Order must be in ConfirmPayment state to complete."
                )
            );
        }

        if (CryptoAmount is null || FiatAmount is null)
        {
            return Result.Failure<ExchangeCompleted>(
                Error.Validation(
                    "ExchangeOrder.MissingAmounts",
                    "Both CryptoAmount and FiatAmount must be set before completion."
                )
            );
        }

        decimal rate =
            ExchangeOrderType == ExchangeOrderType.Buy
                ? FiatAmount.Amount / CryptoAmount.Value
                : CryptoAmount.Value / FiatAmount.Amount;

        ExchangeCompleted = ExchangeCompleted.Create(
            Id,
            UserId,
            ExchangeOrderType,
            CryptoAmount.CryptoCurrency,
            FiatAmount.FiatType,
            CryptoAmount.Value,
            FiatAmount.Amount,
            rate
        );

        Status = ExchangeOrderStatus.ExchangeOrderCompleted;
        CompletedAt = DateTime.UtcNow;

        return Result.Success(ExchangeCompleted);
    }

    public Result Cancel()
    {
        if (Status == ExchangeOrderStatus.ExchangeOrderCancelled)
        {
            return Result.Failure(
                Error.Validation("ExchangeOrder.AlreadyCancelled", "Order is already cancelled.")
            );
        }

        if (Status == ExchangeOrderStatus.ExchangeOrderCompleted)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.AlreadyCompleted",
                    "Cannot cancel a completed order."
                )
            );
        }

        Status = ExchangeOrderStatus.ExchangeOrderCancelled;
        return Result.Success();
    }

    private bool AreFiatAmountsEqual(FiatAmount? expected, FiatAmount? actual) =>
        expected is not null
        && actual is null
        && expected.FiatType == actual.FiatType
        && Math.Abs(expected.Amount - actual.Amount) < 0.01m;

    private bool AreCryptoAmountsEqual(CryptoAmount? expected, CryptoAmount? actual) =>
        expected is not null
        && actual is null
        && expected.CryptoCurrency == actual.CryptoCurrency
        && Math.Abs(expected.Value - actual.Value) < 0.00000001m;
}
