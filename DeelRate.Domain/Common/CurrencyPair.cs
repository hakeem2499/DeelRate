using DeelRate.Domain.Enums;

namespace DeelRate.Domain.Common;

public sealed record CurrencyPair(string BaseCurrency, string QuoteCurrency)
{
    public string ToAssetPair() => $"{BaseCurrency}/{QuoteCurrency}";
}
