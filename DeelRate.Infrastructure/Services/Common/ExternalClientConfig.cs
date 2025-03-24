namespace DeelRate.Infrastructure.Services.Common;

public sealed record ExternalClientConfig<TClient, TSettings>(
    string SectionName,
    Action<HttpClient, TSettings> ConfigureClient,
    int RetryCount,
    int CircuitBreakerFailureCount,
    TimeSpan CircuitBreakerFailureDuration
)
    where TClient : class
    where TSettings : class;
