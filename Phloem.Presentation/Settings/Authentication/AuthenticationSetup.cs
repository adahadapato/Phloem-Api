using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Settings.Identity;
using Phloem.Presentation.Services;

namespace Phloem.Presentation.Settings.Authentication;

/// <summary>
/// Checks the access token on every request and exposes the signed-in user:
/// <list type="number">
///   <item><description><b>Authentication:</b> JWT bearer, checked against the same <see cref="JwtSettings"/> that
///   signs the tokens (issuer, audience, key, lifetime).</description></item>
///   <item><description><b>Authorization:</b> role checks on endpoints (<c>RequireAuthorization</c>,
///   <c>RequireRole</c>).</description></item>
///   <item><description><b>Current user:</b> <see cref="ICurrentUser"/> for the handlers.</description></item>
/// </list>
/// </summary>
public static class AuthenticationSetup
{
    /// <summary>Allowed difference between server clocks when checking expiry.</summary>
    private static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(30);

    // ───────────────────────────── 1. Services ─────────────────────────────

    /// <summary>Registers JWT authentication, authorization and <see cref="ICurrentUser"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemAuthentication(this IServiceCollection services)
    {
        // 1a. Authentication: JWT bearer is the only scheme
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

        // 1b. Its rules come from JwtSettings (read when the first request arrives)
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((options, jwt) => Apply(options, jwt.Value));

        // 1c. Authorization (role policies are added with the endpoints)
        services.AddAuthorization();

        // 1d. The signed-in user, for handlers
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    // ───────────────────────────── 2. Pipeline ─────────────────────────────

    /// <summary>Reads the token (who), then checks access (allowed?). Must come before the endpoints.</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication UsePhloemAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>What a valid token must look like.</summary>
    /// <param name="options">JWT bearer's options.</param>
    /// <param name="jwt">The <c>Jwt</c> section.</param>
    private static void Apply(JwtBearerOptions options, JwtSettings jwt)
    {
        // Keep short claim names ("sub", "role") exactly as JwtTokenGenerator wrote them
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.CreateSigningKey(),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

            ValidateLifetime = true,
            ClockSkew = ClockSkew,

            NameClaimType = JwtClaimNames.UserId,
            RoleClaimType = JwtClaimNames.Role
        };
    }
}