namespace DeelRate.Infrastructure.Services.CoinApiClient;

public class CoinApiResponse
{
    public string AssetIdQuote { get; set; } = string.Empty;
    public string AssetIdBase { get; set; } = string.Empty;
    public decimal Rate { get; set; } = 0;

    public DateTime Time { get; set; } = DateTime.MinValue;
}
