using DeelRate.Api;
using DeelRate.Api.Extensions;
using DeelRate.Application;
using DeelRate.Application.Abstractions.Services;
using DeelRate.Domain.Common;
using DeelRate.Infrastructure;
using DeelRate.Infrastructure.Services.CoinApiClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration)
);

// Add OpenAPI services (for Scalar)
builder.Services.AddOpenApi();

// Add services to the container through extension methods
builder
    .Services.AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddPresentation(builder.Configuration);

// Add authorization
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseSerilogRequestLogging();
app.UseRequestContextLogging();
app.UseExceptionHandler();

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
    "/exchange-rate/",
    async (IExchangeRateService service) =>
    {
        var pairs = new List<CurrencyPair>
        {
            new("XRP", "USDC"),
            new("BTC", "USDT"),
            new("ETH", "USDT"),
        };
        Result<List<ExchangeRate>> result = await service.GetExchangeRatesAsync(pairs);

        return result.IsSuccess
            ? Results.Ok(
                result.Value.Select(r => new
                {
                    Pair = r.CurrencyPair.ToAssetPair(),
                    r.Rate,
                    Time = r.Timestamp,
                })
            )
            : Results.BadRequest(result.Error);
    }
);

app.Run();
