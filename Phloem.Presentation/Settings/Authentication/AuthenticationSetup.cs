using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Infrastructure.Settings.Identity;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Services;

namespace Phloem.Presentation.Settings.Authentication;

/// <summary>
/// Checks the access token on every request and exposes the signed-in user:
/// <list type="number">
///   <item><description><b>Authentication:</b> JWT bearer, checked against the same <see cref="JwtSettings"/> that
///   signs the tokens (issuer, audience, key, lifetime). "Continue with Google" is added by
///   <see cref="GoogleAuthSetup"/> when its keys are set.</description></item>
///   <item><description><b>Refusals:</b> 401 / 403 written as ProblemDetails with a <c>code</c> (no token, expired,
///   invalid, not allowed), the same shape as every other error.</description></item>
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

    /// <summary>Registers JWT authentication (plus Google when its keys are set), authorization and
    /// <see cref="ICurrentUser"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration (the <c>Authentication</c> section).</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemAuthentication(this IServiceCollection services,
                                                             IConfiguration configuration)
    {
        // 1a. Authentication: JWT bearer is the default (and only sign-in) scheme. Google only proves who someone is
        //     on the way in; the API then hands out its own JWT, exactly as after a password login.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer()
                .AddPhloemGoogle(configuration);   // off while Authentication:Google has no keys

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

    /// <summary>What a valid token must look like, and how refusals are written.</summary>
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

        // Refusals as ProblemDetails (by default they're empty)
        options.Events = new JwtBearerEvents
        {
            OnChallenge = WriteUnauthorizedAsync,
            OnForbidden = WriteForbiddenAsync
        };
    }

    /// <summary>401: no token, an expired one, or an invalid one, each with its own <c>code</c>.</summary>
    private static Task WriteUnauthorizedAsync(JwtBearerChallengeContext context)
    {
        context.HandleResponse();   // replace the default empty 401

        var error = context.AuthenticateFailure switch
        {
            null => AuthErrors.SignInRequired,
            SecurityTokenExpiredException => AuthErrors.SessionExpired,
            _ => AuthErrors.InvalidAccessToken
        };

        // Standard header so clients know which scheme is expected
        context.Response.Headers.WWWAuthenticate = context.AuthenticateFailure is null
            ? "Bearer"
            : "Bearer error=\"invalid_token\"";

        return error.ToProblem().ExecuteAsync(context.HttpContext);
    }

    /// <summary>403: signed in, but the endpoint needs a role this user doesn't have.</summary>
    private static Task WriteForbiddenAsync(ForbiddenContext context) =>
        AuthErrors.NotAllowed.ToProblem().ExecuteAsync(context.HttpContext);
}