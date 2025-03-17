using System.ComponentModel.DataAnnotations;

namespace DeelRate.Api;

public class KindeSettings
{
    [Required]
    public const string ConfigurationSectionName = "KindeSettings";

    [Required]
    public string Domain { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    [Required]
    public string RedirectUri { get; init; } = string.Empty;

    [Required]
    public string ResponseType { get; init; } = string.Empty;

    [Required]
    public string LogoutRedirectUri { get; init; } = string.Empty;
}
