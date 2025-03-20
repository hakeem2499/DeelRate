using System.ComponentModel.DataAnnotations;

namespace DeelRate.Infrastructure.Services.DepositAddressProvider;

public sealed class FiatDepositAddressProviderSettings
{
    public const string ConfigurationSectionName = "FiatDepositAddressProviderSettings";

    [Required]
    public string EUR_AccountNumber { get; init; } = string.Empty;

    [Required]
    public string EUR_AccountName { get; init; } = string.Empty;

    [Required]
    public string EUR_BankAccount { get; init; } = string.Empty;

    [Required]
    public string USD_AccountNumber { get; init; } = string.Empty;

    [Required]
    public string USD_AccountName { get; init; } = string.Empty;

    [Required]
    public string USD_BankAccount { get; init; } = string.Empty;

    [Required]
    public string NGN_AccountNumber { get; init; } = string.Empty;

    [Required]
    public string NGN_AccountName { get; init; } = string.Empty;

    [Required]
    public string NGN_BankAccount { get; init; } = string.Empty;
}
