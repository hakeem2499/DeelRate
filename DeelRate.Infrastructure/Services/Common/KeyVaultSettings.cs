using System.ComponentModel.DataAnnotations;

namespace DeelRate.Infrastructure.Services.Common;

public class KeyVaultSettings
{
    public const string ConfigurationSectionName = "KeyVaultSettings";

    [Required(ErrorMessage = "Key Vault name is required.")]
    public string KeyVaultName { get; init; } = string.Empty;

    public string KeyVaultUri => $"https://{KeyVaultName}.vault.azure.net/";
}