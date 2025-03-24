using DeelRate.Api.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace DeelRate.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddKindeAuthentication(configuration);
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddKindeAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<KindeSettings>()
            .Bind(configuration.GetSection(KindeSettings.ConfigurationSectionName))
            .ValidateDataAnnotations() // Validate using data annotations
            .ValidateOnStart(); // Ensure validation happens on startup
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/auth/login"; // Redirect unauthenticated users here
                options.LogoutPath = "/auth/logout";
            })
            .AddOpenIdConnect(options =>
            {
                KindeSettings? kindeSettings = configuration
                    .GetSection(KindeSettings.ConfigurationSectionName)
                    .Get<KindeSettings>();

                options.Authority = kindeSettings!.Domain; // Use "Domain" to match Kinde's issuer
                options.ClientId = kindeSettings.ClientId;
                options.ClientSecret = kindeSettings.ClientSecret;
                options.ResponseType = kindeSettings.ResponseType; // Authorization code flow
                options.SaveTokens = true; // Store tokens for later use
                options.CallbackPath = kindeSettings.RedirectUri; // Must match Kinde's Redirect URI
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.GetClaimsFromUserInfoEndpoint = true;

                // Map Kinde claims
                options.TokenValidationParameters.NameClaimType = "name";
            });

        return services;
    }
}
