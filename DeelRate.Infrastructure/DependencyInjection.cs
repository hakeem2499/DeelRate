using DeelRate.Application.Abstractions.Services;
using DeelRate.Domain.Common;
using DeelRate.Infrastructure.Services.CheckCryptoAddressClient;
using DeelRate.Infrastructure.Services.CoinApiClient;
using DeelRate.Infrastructure.Services.Exchange;
using DeelRate.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Refit;

namespace DeelRate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure and validate settings
        services
            .AddOptions<CoinApiSettings>()
            .Bind(configuration.GetSection(CoinApiSettings.ConfigurationSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<CryptoAddressSettings>()
            .Bind(configuration.GetSection(CryptoAddressSettings.ConfigurationSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Configure Refit to use Newtonsoft.Json
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer(
                new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                    {
                        NamingStrategy =
                            new Newtonsoft.Json.Serialization.SnakeCaseNamingStrategy(),
                    },
                }
            ),
        };

        // Register Refit clients
        AddRefitClient<ICoinApi>(
            services,
            refitSettings,
            httpClient =>
            {
                CoinApiSettings? settings = configuration
                    .GetSection(CoinApiSettings.ConfigurationSectionName)
                    .Get<CoinApiSettings>();
                httpClient.BaseAddress = new Uri(settings!.BaseAddress);
                httpClient.DefaultRequestHeaders.Add("X-CoinAPI-Key", settings.ApiKey);
                httpClient.DefaultRequestHeaders.Add("Accept", settings.ResponseType);
            }
        );

        AddRefitClient<ICryptoAddress>(
            services,
            refitSettings,
            httpClient =>
            {
                CryptoAddressSettings? settings = configuration
                    .GetSection(CryptoAddressSettings.ConfigurationSectionName)
                    .Get<CryptoAddressSettings>();
                httpClient.BaseAddress = new Uri(settings!.BaseAddress);
                httpClient.DefaultRequestHeaders.Add("X-Api-Key", settings.ApiKey);
            }
        );

        // Register other services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddMemoryCache();

        return services;
    }

    private static void AddRefitClient<T>(
        IServiceCollection services,
        RefitSettings refitSettings,
        Action<HttpClient> configureClient
    )
        where T : class =>
        services
            .AddRefitClient<T>(refitSettings)
            .ConfigureHttpClient(configureClient)
            .AddHttpMessageHandler(provider =>
            {
                ILogger<T> logger = provider.GetRequiredService<ILogger<T>>();
                return new LoggingHandler(logger);
            })
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                )
            )
            .AddTransientHttpErrorPolicy(policy =>
                policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
            );
}

public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Sending request to {Uri}", request.RequestUri);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation(
            "Received response with status code {StatusCode}",
            response.StatusCode
        );

        return response;
    }
}
