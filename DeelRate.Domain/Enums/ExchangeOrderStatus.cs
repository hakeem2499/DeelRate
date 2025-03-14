namespace DeelRate.Domain.Enums
{
    public enum ExchangeOrderStatus
    {
        Initiated = 0,
        PaymentPending = 1,
        PaymentCompleted = 2,
        ConfirmPayment = 3,
        ExchangeOrderCompleted = 4,
        ExchangeOrderCancelled = 5,
    }
}
