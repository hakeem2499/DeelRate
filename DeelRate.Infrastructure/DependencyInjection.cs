using System.Net.Http.Headers;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using DeelRate.Application.Abstractions.Services;
using DeelRate.Domain.Common;
using DeelRate.Infrastructure.Services.BrevoEmailServiceClient;
using DeelRate.Infrastructure.Services.CheckCryptoAddressClient;
using DeelRate.Infrastructure.Services.CoinApiClient;
using DeelRate.Infrastructure.Services.Common;
using DeelRate.Infrastructure.Services.DepositAddressProvider;
using DeelRate.Infrastructure.Services.Exchange;
using DeelRate.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Refit;

namespace DeelRate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false
    )
    {
        // Configure and validate settings
        services
            .ConfigureServiceSettings<CoinApiSettings>(
                CoinApiSettings.ConfigurationSectionName,
                configuration
            )
            .ConfigureServiceSettings<CryptoAddressSettings>(
                CryptoAddressSettings.ConfigurationSectionName,
                configuration
            )
            .ConfigureServiceSettings<CryptoDepositAddressProviderSettings>(
                CryptoDepositAddressProviderSettings.ConfigurationSectionName,
                configuration
            )
            .ConfigureServiceSettings<FiatDepositAddressProviderSettings>(
                FiatDepositAddressProviderSettings.ConfigurationSectionName,
                configuration
            );

        // Conditionally add Azure Key Vault and other production-specific configurations
        if (!isDevelopment)
        {
            services.AddAzureKeyVault(configuration);
        }

        // Add external clients and OpenTelemetry
        services.AddExternalClients().AddOpenTelemetryServices(isDevelopment);

        // Register other services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddMemoryCache();

        return services;
    }

    private static IServiceCollection ConfigureServiceSettings<TSettings>(
        this IServiceCollection services,
        string sectionName,
        IConfiguration configuration
    )
        where TSettings : class
    {
        services
            .AddOptions<TSettings>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddAzureKeyVault(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Bind and validate KeyVaultSettings
        services.ConfigureServiceSettings<KeyVaultSettings>(
            KeyVaultSettings.ConfigurationSectionName,
            configuration
        );

        // Configure Azure Key Vault
        services.AddSingleton(provider =>
        {
            KeyVaultSettings keyVaultSettings = provider
                .GetRequiredService<IOptions<KeyVaultSettings>>()
                .Value;

            // Add Azure Key Vault to the configuration
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddAzureKeyVault(
                    new Uri(keyVaultSettings.KeyVaultUri),
                    new DefaultAzureCredential()
                );

            return configBuilder.Build();
        });

        return services;
    }

    private static IServiceCollection AddExternalClients(this IServiceCollection services)
    {
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

        services.AddExternalClient(
            new ExternalClientConfig<ICoinApi, CoinApiSettings>(
                CoinApiSettings.ConfigurationSectionName,
                (client, settings) =>
                {
                    client.BaseAddress = new Uri(settings.BaseAddress);
                    client.DefaultRequestHeaders.Add("X-CoinAPI-Key", settings.ApiKey);
                    client.DefaultRequestHeaders.Add("Accept", settings.ResponseType);
                },
                RetryCount: 3,
                CircuitBreakerFailureCount: 5,
                CircuitBreakerFailureDuration: TimeSpan.FromSeconds(30)
            ),
            refitSettings
        );

        services.AddExternalClient(
            new ExternalClientConfig<ICryptoAddress, CryptoAddressSettings>(
                CryptoAddressSettings.ConfigurationSectionName,
                (client, settings) =>
                {
                    client.BaseAddress = new Uri(settings.BaseAddress);
                    client.DefaultRequestHeaders.Add("X-CoinAPI-Key", settings.ApiKey);
                },
                RetryCount: 2,
                CircuitBreakerFailureCount: 3,
                CircuitBreakerFailureDuration: TimeSpan.FromSeconds(15)
            ),
            refitSettings
        );
        services.AddExternalClient(
            new ExternalClientConfig<IBrevoClient, BrevoApiSettings>(
                BrevoApiSettings.ConfigurationSectionName,
                (client, settings) =>
                {
                    client.BaseAddress = new Uri(settings.BaseAddress);
                    client.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(settings.AcceptHeader)
                    );
                },
                RetryCount: 2,
                CircuitBreakerFailureCount: 3,
                CircuitBreakerFailureDuration: TimeSpan.FromSeconds(15)
            ),
            refitSettings
        );

        return services;
    }

    private static void AddExternalClient<TClient, TSettings>(
        this IServiceCollection services,
        ExternalClientConfig<TClient, TSettings> config,
        RefitSettings refitSettings
    )
        where TClient : class
        where TSettings : class
    {
        services
            .AddRefitClient<TClient>(refitSettings)
            .ConfigureHttpClient(
                (sp, client) =>
                {
                    TSettings settings = sp.GetRequiredService<IOptions<TSettings>>().Value;
                    config.ConfigureClient(client, settings);
                }
            )
            .AddHttpMessageHandler(sp => new LoggingHandler(
                sp.GetRequiredService<ILogger<TClient>>()
            ))
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(
                    config.RetryCount,
                    retry => TimeSpan.FromSeconds(Math.Pow(2, retry))
                )
            )
            .AddTransientHttpErrorPolicy(policy =>
                policy.CircuitBreakerAsync(
                    config.CircuitBreakerFailureCount,
                    config.CircuitBreakerFailureDuration
                )
            );
    }

    private static void AddOpenTelemetryServices(
        this IServiceCollection services,
        bool isDevelopment
    )
    {
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService("DeelRate.CryptoExchange", serviceVersion: "1.0.0")
            )
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("DeelRate"); // For custom traces

                if (isDevelopment)
                {
                    tracing.AddConsoleExporter(); // For development
                }
                else
                {
                    // Use OTLP or another exporter for production
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://your-otlp-endpoint:4317");
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation(); // Add runtime metrics

                if (isDevelopment)
                {
                    metrics.AddConsoleExporter(); // For development
                }
                else
                {
                    // Use Prometheus or another exporter for production
                    metrics.AddPrometheusExporter();
                }
            });

        // Enhance logging with OpenTelemetry
        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DeelRate"));

                if (isDevelopment)
                {
                    options.AddConsoleExporter(); // For development
                }
                else
                {
                    // Use OTLP or another exporter for production
                    options.AddOtlpExporter();
                }
            });
        });
    }
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
