namespace DeelRate.Domain.Common;

public record CurrencyPair(string BaseCurrency, string QuoteCurrency)
{
    public string ToAssetPair() => $"{BaseCurrency}/{QuoteCurrency}";
}
