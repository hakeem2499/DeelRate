using DeelRate.Domain.Common;
using DeelRate.Infrastructure.Services.CoinApiClient;
using DeelRate.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Refit;

namespace DeelRate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure CoinApiSettings from appsettings.json
        services.Configure<CoinApiSettings>(
            configuration.GetSection(CoinApiSettings.ConfigurationSectionName)
        );

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

        // Register the Refit client
        services
            .AddRefitClient<ICoinApi>(refitSettings) // Pass the Refit settings here
            .ConfigureHttpClient(
                (sp, httpClient) =>
                {
                    CoinApiSettings settings = sp.GetRequiredService<
                        IOptions<CoinApiSettings>
                    >().Value;
                    httpClient.BaseAddress = new Uri(settings.BaseAddress);
                    httpClient.DefaultRequestHeaders.Add("X-CoinAPI-Key", settings.ApiKey);
                    httpClient.DefaultRequestHeaders.Add("Accept", settings.ResponseType);
                }
            );

        // Register other services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
