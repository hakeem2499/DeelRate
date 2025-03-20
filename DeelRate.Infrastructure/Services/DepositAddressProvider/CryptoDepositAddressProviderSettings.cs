using System.ComponentModel.DataAnnotations;

namespace DeelRate.Infrastructure.Services.DepositAddressProvider;

public class CryptoDepositAddressProviderSettings
{
    public const string ConfigurationSectionName = "CryptoDepositAddressProviderSettings";

    [Required]
    public string BTC_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string ETH_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string USDT_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string USDC_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string BUSD_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string BNB_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string ADA_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string DOGE_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string XRP_WalletAddress { get; init; } = string.Empty;

    [Required]
    public string SOL_WalletAddress { get; init; } = string.Empty;
}
