using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI services (for Scalar)
builder.Services.AddOpenApi();

// Configure authentication with Kinde
var kindeSettings = builder.Configuration.GetSection("Kinde");
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
        options.Authority = kindeSettings["Authority"]; // Use "Domain" to match Kinde's issuer
        options.ClientId = kindeSettings["ClientId"];
        options.ClientSecret = kindeSettings["ClientSecret"];
        options.ResponseType = "code"; // Authorization code flow
        options.SaveTokens = true; // Store tokens for later use
        options.CallbackPath = kindeSettings["RedirectUri"]; // Must match Kinde's Redirect URI
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.GetClaimsFromUserInfoEndpoint = true;

        // Map Kinde claims
        options.TokenValidationParameters.NameClaimType = "name";
    });

// Add authorization (optional, for future use)
builder.Services.AddAuthorization();

var app = builder.Build();

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
    (HttpContext context, string returnUrl = "/") =>
    {
        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = returnUrl },
            new[] { OpenIdConnectDefaults.AuthenticationScheme }
        );
    }
);

app.MapGet(
    "/auth/logout",
    async (HttpContext context) =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return Results.Redirect(
            $"{kindeSettings["Authority"]}/logout?redirect={kindeSettings["RedirectUri"]}"
        );
    }
);

// Test endpoint to verify authentication
app.MapGet(
    "/auth/profile",
    [Authorize]
    (HttpContext context) =>
    {
        var user = context.User;
        return Results.Ok(
            new
            {
                Name = user.Identity?.Name,
                Email = user.FindFirst("email")?.Value,
                UserId = user.FindFirst("sub")?.Value,
            }
        );
    }
);

app.Run();
