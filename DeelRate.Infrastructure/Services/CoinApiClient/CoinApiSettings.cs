using System.ComponentModel.DataAnnotations;

namespace DeelRate.Infrastructure.Services.CoinApiClient;

public class CoinApiSettings
{
    [Required]
    public const string ConfigurationSectionName = "CoinApiSettings";

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required, Url]
    public string BaseAddress { get; init; } = string.Empty;

    [Required]
    public string ResponseType { get; init; } = string.Empty;
}
