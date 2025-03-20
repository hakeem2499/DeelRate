using System;
using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;
using DeelRate.Domain.ValueObjects;

namespace DeelRate.Domain.Entities;

public class ExchangeOrder : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public CryptoType CryptoType { get; private set; }
    public ExchangeOrderType ExchangeOrderType { get; private set; }
    public CryptoAmount? CryptoAmount { get; private set; }
    public FiatAmount? FiatAmount { get; private set; }
    public ExchangeOrderStatus Status { get; private set; }
    public DestinationAddress UserDestinationAddress { get; private set; }
    public DestinationAddress SystemDepositAddress { get; private set; }
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
        DestinationAddress userDestinationAddress,
        DestinationAddress systemDepositAddress
    )
    {
        Result validationResult = ValidateInitiation(
            userId,
            cryptoType,
            exchangeOrderType,
            cryptoAmount,
            fiatAmount,
            userDestinationAddress,
            systemDepositAddress
        );

        if (validationResult.IsFailure)
        {
            return (Result<ExchangeOrder>)validationResult;
        }

        var order = new ExchangeOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CryptoType = cryptoType,
            ExchangeOrderType = exchangeOrderType,
            CryptoAmount = cryptoAmount, // Null for Buy, set for Sell
            FiatAmount = fiatAmount, // Set for Buy, null for Sell
            UserDestinationAddress = userDestinationAddress,
            SystemDepositAddress = systemDepositAddress,
            Status = ExchangeOrderStatus.Initiated,
            CreatedAt = DateTime.UtcNow,
        };

        return Result.Success(order);
    }

    public Result<ExchangeOrder> UpdateExchangeOrder(
        CryptoAmount? cryptoAmount,
        FiatAmount? fiatAmount,
        DestinationAddress userDestinationAddress,
        DestinationAddress systemDepositAddress
    )
    {
        if (Status != ExchangeOrderStatus.Initiated)
        {
            return Result.Failure<ExchangeOrder>(
                Error.Validation(
                    "ExchangeOrder.NotInitiated",
                    "Order must be in Initiated state to update."
                )
            );
        }

        Result validationResult = ValidateInitiation(
            UserId,
            CryptoType,
            ExchangeOrderType,
            cryptoAmount,
            fiatAmount,
            userDestinationAddress,
            systemDepositAddress
        );

        if (validationResult.IsFailure)
        {
            return (Result<ExchangeOrder>)validationResult;
        }

        CryptoAmount = cryptoAmount;
        FiatAmount = fiatAmount;
        UserDestinationAddress = userDestinationAddress;
        SystemDepositAddress = systemDepositAddress;

        return Result.Success(this);
    }

    private static Result ValidateInitiation(
        Guid userId,
        CryptoType cryptoType,
        ExchangeOrderType exchangeOrderType,
        CryptoAmount? cryptoAmount,
        FiatAmount? fiatAmount,
        DestinationAddress userDestinationAddress,
        DestinationAddress systemDepositAddress
    )
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(
                Error.Validation("ExchangeOrder.InvalidUserId", "User ID must be a valid GUID.")
            );
        }

        if (!Enum.IsDefined(typeof(ExchangeOrderType), exchangeOrderType))
        {
            return Result.Failure(
                Error.Validation("ExchangeOrder.InvalidType", "Invalid exchange order type.")
            );
        }

        if (!Enum.IsDefined(typeof(CryptoType), cryptoType))
        {
            return Result.Failure(
                Error.Validation("ExchangeOrder.InvalidCryptoType", "Invalid cryptocurrency type.")
            );
        }

        return exchangeOrderType switch
        {
            ExchangeOrderType.Buy => ValidateBuyOrder(
                fiatAmount,
                cryptoAmount,
                userDestinationAddress,
                systemDepositAddress
            ),
            ExchangeOrderType.Sell => ValidateSellOrder(
                cryptoAmount,
                fiatAmount,
                userDestinationAddress,
                systemDepositAddress
            ),
            _ => Result.Failure(
                Error.Validation("ExchangeOrder.InvalidType", "Invalid exchange order type.")
            ),
        };
    }

    private static Result ValidateBuyOrder(
        FiatAmount? fiatAmount,
        CryptoAmount? cryptoAmount,
        DestinationAddress userDestinationAddress,
        DestinationAddress systemDepositAddress
    )
    {
        if (fiatAmount is null)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.MissingFiatAmount",
                    "FiatAmount is required to buy crypto."
                )
            );
        }

        if (cryptoAmount is not null)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.UnexpectedCryptoAmount",
                    "CryptoAmount should not be provided for a Buy order."
                )
            );
        }

        if (userDestinationAddress.DestinationAddressType != AddressType.CryptoDepositAddress)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.InvalidDestination",
                    "For a Buy order, destination must be a crypto address."
                )
            );
        }

        if (systemDepositAddress.DestinationAddressType != AddressType.FiatAccountNumber)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.InvalidSystemDeposit",
                    "For a Buy order, system deposit must be a fiat account."
                )
            );
        }

        return Result.Success();
    }

    private static Result ValidateSellOrder(
        CryptoAmount? cryptoAmount,
        FiatAmount? fiatAmount,
        DestinationAddress userDestinationAddress,
        DestinationAddress systemDepositAddress
    )
    {
        if (cryptoAmount is null)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.MissingCryptoAmount",
                    "CryptoAmount is required to sell crypto."
                )
            );
        }

        if (fiatAmount is not null)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.UnexpectedFiatAmount",
                    "FiatAmount should not be provided for a Sell order."
                )
            );
        }

        if (userDestinationAddress.DestinationAddressType != AddressType.FiatAccountNumber)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.InvalidDestination",
                    "For a Sell order, destination must be a fiat account."
                )
            );
        }

        if (systemDepositAddress.DestinationAddressType != AddressType.CryptoDepositAddress)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.InvalidSystemDeposit",
                    "For a Sell order, system deposit must be a crypto address."
                )
            );
        }

        return Result.Success();
    }

    public Result MarkPaymentPending()
    {
        if (Status is not ExchangeOrderStatus.Initiated)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.NotInitiated",
                    "Order must be in Initiated state to mark payment as pending."
                )
            );
        }

        Status = ExchangeOrderStatus.PaymentPending;
        return Result.Success();
    }

    public Result SystemConfirmPayment(
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
                    "Order must be in PaymentPending state for system confirmation."
                )
            );
        }

        if (rate <= 0)
        {
            return Result.Failure(
                Error.Validation("ExchangeOrder.InvalidRate", "Exchange rate must be positive.")
            );
        }

        Result validationResult = ExchangeOrderType switch
        {
            ExchangeOrderType.Buy => ValidateBuyPayment(actualFiatReceived),
            ExchangeOrderType.Sell => ValidateSellPayment(actualCryptoReceived),
            _ => Result.Failure(
                Error.Validation("ExchangeOrder.InvalidType", "Invalid exchange order type.")
            ),
        };

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        if (ExchangeOrderType == ExchangeOrderType.Buy)
        {
            CryptoAmount = new CryptoAmount(actualFiatReceived!.Amount / rate, CryptoType);
        }
        else if (ExchangeOrderType == ExchangeOrderType.Sell)
        {
            FiatAmount = new FiatAmount(FiatAmount!.FiatType, actualCryptoReceived!.Value * rate);
        }

        Status = ExchangeOrderStatus.SystemConfirmedPayment;
        return Result.Success();
    }

    private Result ValidateBuyPayment(FiatAmount? actualFiatReceived)
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

        return Result.Success();
    }

    private Result ValidateSellPayment(CryptoAmount? actualCryptoReceived)
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

        return Result.Success();
    }

    public Result UserConfirmPayment()
    {
        if (Status != ExchangeOrderStatus.SystemConfirmedPayment)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.NotSystemConfirmed",
                    "Order must be system-confirmed before user confirmation."
                )
            );
        }

        if (CryptoAmount is null || FiatAmount is null)
        {
            return Result.Failure(
                Error.Validation(
                    "ExchangeOrder.MissingAmounts",
                    "Both CryptoAmount and FiatAmount must be set before user confirmation."
                )
            );
        }

        Status = ExchangeOrderStatus.UserConfirmPayment;
        return Result.Success();
    }

    public Result<ExchangeCompleted> CompleteExchange()
    {
        if (Status != ExchangeOrderStatus.UserConfirmPayment)
        {
            return Result.Failure<ExchangeCompleted>(
                Error.Validation(
                    "ExchangeOrder.NotUserConfirmed",
                    "Order must be user-confirmed to complete."
                )
            );
        }

        decimal rate =
            ExchangeOrderType == ExchangeOrderType.Buy
                ? FiatAmount!.Amount / CryptoAmount!.Value
                : CryptoAmount!.Value / FiatAmount!.Amount;

        ExchangeCompleted = ExchangeCompleted.Create(
            Id,
            UserId,
            ExchangeOrderType,
            CryptoAmount!.CryptoCurrency,
            FiatAmount!.FiatType,
            CryptoAmount!.Value,
            FiatAmount!.Amount,
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

    private static bool AreFiatAmountsEqual(FiatAmount? expected, FiatAmount? actual)
    {
        return expected is not null
            && actual is not null
            && expected.FiatType == actual.FiatType
            && Math.Abs(expected.Amount - actual.Amount) < 0.01m;
    }

    private static bool AreCryptoAmountsEqual(CryptoAmount? expected, CryptoAmount? actual)
    {
        return expected is not null
            && actual is not null
            && expected.CryptoCurrency == actual.CryptoCurrency
            && Math.Abs(expected.Value - actual.Value) < 0.00000001m;
    }
}
