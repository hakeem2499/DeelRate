using DeelRate.Domain.Common;
using DeelRate.Domain.Enums;

namespace DeelRate.Domain.Entities
{
    public class ExchangeCompleted : Entity
    {
        public ExchangeCompleted() { }

        public ExchangeCompleted(
            Guid exchangeOrderId,
            Guid userId,
            ExchangeOrderType exchangeOrderType,
            CryptoType cryptoType,
            FiatType fiatType,
            decimal amountFrom,
            decimal amountTo,
            decimal rate
        )
        {
            ExchangeOrderId = exchangeOrderId;
            UserId = userId;
            ExchangeOrderType = exchangeOrderType;
            CryptoType = cryptoType;
            FiatType = fiatType;
            AmountFrom = amountFrom;
            AmountTo = amountTo;
            Rate = rate;
        }

        public Guid ExchangeOrderId { get; private set; }
        public Guid UserId { get; private set; }
        public ExchangeOrderType ExchangeOrderType { get; private set; }
        public CryptoType CryptoType { get; private set; }
        public FiatType FiatType { get; private set; }
        public decimal AmountFrom { get; private set; }
        public decimal AmountTo { get; private set; }
        public decimal Rate { get; private set; }

        internal static ExchangeCompleted Create(
            Guid exchangeOrderId,
            Guid userId,
            ExchangeOrderType exchangeOrderType,
            CryptoType cryptoType,
            FiatType fiatType,
            decimal amountFrom,
            decimal amountTo,
            decimal rate
        )
        {
            return new ExchangeCompleted(
                exchangeOrderId,
                userId,
                exchangeOrderType,
                cryptoType,
                fiatType,
                amountFrom,
                amountTo,
                rate
            );
        }
    }
}
