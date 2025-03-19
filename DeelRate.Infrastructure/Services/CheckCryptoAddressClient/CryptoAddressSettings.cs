using System.ComponentModel.DataAnnotations;

namespace DeelRate.Infrastructure.Services.CheckCryptoAddressClient;

public class CryptoAddressSettings
{
    [Required]
    public const string ConfigurationSectionName = "CryptoAddressSettings";

    [Required]
    public string BaseAddress { get; init; } = string.Empty;

    [Required]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public string ContentType { get; init; } = "application/json";
}
