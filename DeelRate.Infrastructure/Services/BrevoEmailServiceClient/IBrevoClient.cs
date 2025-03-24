using Newtonsoft.Json;
using Refit;

namespace DeelRate.Infrastructure.Services.BrevoEmailServiceClient;

public interface IBrevoClient
{
    [Post("/v3/smtp/email")]
    Task<ApiResponse<object>> SendTemplatedEmailAsync([Body] BrevoEmailRequest request);
}

public record BrevoEmailRequest(
    [property: JsonProperty("to")] List<BrevoRecipient> ToEmail,
    [property: JsonProperty("templateId")] int TemplateId,
    [property: JsonProperty("params")] Dictionary<string, object> Parameters
);

public record BrevoRecipient(
    [property: JsonProperty("email")] string Email,
    [property: JsonProperty("name")] string Name
);
