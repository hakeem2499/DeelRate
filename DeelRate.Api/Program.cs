using DeelRate.Api;
using DeelRate.Infrastructure;
using DeelRate.Infrastructure.Services.CoinApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services (for Scalar)
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

// Bind KindeSettings from configuration
builder
    .Services.AddOptions<KindeSettings>()
    .Bind(builder.Configuration.GetSection("KindeSettings"))
    .ValidateDataAnnotations() // Validate using data annotations
    .ValidateOnStart(); // Ensure validation happens on startup

// Configure authentication with Kinde
builder
    .Services.AddAuthentication(options =>
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
        KindeSettings? kindeSettings = builder
            .Configuration.GetSection("KindeSettings")
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

// Add authorization
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Use HTTPS redirection (disable in dev if not using HTTPS locally)
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Authentication endpoints
app.MapGet(
    "/auth/login",
    (HttpContext context, string returnUrl = "/dashboard") =>
    {
        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            new[] { OpenIdConnectDefaults.AuthenticationScheme }
        );
    }
);

app.MapGet(
    "/auth/logout",
    async (HttpContext context, IOptions<KindeSettings> kindeSettings) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return Results.Redirect(
            $"{kindeSettings.Value.Domain}/logout?redirect={kindeSettings.Value.LogoutRedirectUri}"
        );
    }
);

// Test endpoint to verify authentication
app.MapGet(
    "/auth/profile",
    [Authorize]
    (HttpContext context) =>
    {
        System.Security.Claims.ClaimsPrincipal user = context.User;
        return Results.Ok(
            new
            {
                user.Identity?.Name,
                Email = user.FindFirst("email")?.Value,
                UserId = user.FindFirst("sub")?.Value,
            }
        );
    }
);

app.MapGet(
    "/exchange-rate/{coin}/{currency}",
    async (ICoinApi coinApi, string coin, string currency) =>
    {
        CoinApiResponse? response = await coinApi.GetCoinExchangeRateAsync(coin, currency);

        return Results.Ok(response);
    }
);

app.Run();
