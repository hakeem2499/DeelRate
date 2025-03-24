using DeelRate.Application.Abstractions.Services;
using DeelRate.Infrastructure.Services.BrevoEmailServiceClient;

namespace DeelRate.Infrastructure.Services.Messaging;

public class BrevoEmailSenderService(IBrevoClient brevoClient) : IBrevoEmailSender
{
    private readonly IBrevoClient _brevoClient = brevoClient;

    public async Task SendTemplatedEmailAsync(
        string toEmail,
        string toName,
        int templateId,
        Dictionary<string, object> parameters,
        CancellationToken ct = default
    )
    {
        var request = new BrevoEmailRequest(
            ToEmail: new List<BrevoRecipient> { new BrevoRecipient(Email: toEmail, Name: toName) },
            TemplateId: templateId,
            Parameters: parameters
        );

        await _brevoClient.SendTemplatedEmailAsync(request);
    }
}
