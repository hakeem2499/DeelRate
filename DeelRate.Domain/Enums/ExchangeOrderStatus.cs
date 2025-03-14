namespace DeelRate.Domain.Enums;

public enum ExchangeOrderStatus
{
    Initiated = 0,
    PaymentPending = 1,
    SystemConfirmedPayment = 2,
    UserConfirmPayment = 3,
    ExchangeOrderCompleted = 4,
    ExchangeOrderCancelled = 5,
}
