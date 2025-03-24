using System.ComponentModel.DataAnnotations;

namespace DeelRate.Infrastructure.Services.BrevoEmailServiceClient;

public class BrevoApiSettings
{
    public const string ConfigurationSectionName = "BrevoApiSettings";

    [Required(ErrorMessage = "Brevo API base URL is required.")]
    public string BaseAddress { get; init; } = string.Empty;

    [Required(ErrorMessage = "Brevo API key is required.")]
    public string ApiKey { get; init; } = string.Empty;

    [Required]
    public string FromName { get; init; } = string.Empty;

    [Required]
    public string FromEmail { get; init; } = string.Empty;

    public string AcceptHeader { get; init; } = "application/json";
}
