namespace DeelRate.Application.Abstractions.Services;

public interface IBrevoEmailSender
{
    Task SendTemplatedEmailAsync(
        string toEmail,
        string toName,
        int templateId,
        Dictionary<string, object> parameters,
        CancellationToken ct = default
    );
}
