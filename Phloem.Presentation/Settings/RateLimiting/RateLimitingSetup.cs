using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Phloem.Presentation.Settings.RateLimiting;

/// <summary>
/// Request limits (ASP.NET Core's built-in rate limiter, no extra package), for endpoints anyone can call that
/// could be abused. Today: sending a Contact us message.
/// </summary>
/// <remarks>
/// Counted per connection address. A refused request gets 429 with a ProblemDetails message the website and app
/// show as it is.
/// </remarks>
public static class RateLimitingSetup
{
    /// <summary>Policy name for <c>POST /api/contact/messages</c>.</summary>
    public const string ContactPolicy = "contact";

    /// <summary>Messages allowed per window.</summary>
    public const int ContactPermitLimit = 5;

    /// <summary>The window.</summary>
    public static readonly TimeSpan ContactWindow = TimeSpan.FromMinutes(10);

    /// <summary>Registers the policies.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(ContactPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = ContactPermitLimit,
                        Window = ContactWindow,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too many requests",
                    status = StatusCodes.Status429TooManyRequests,
                    code = "RateLimit.TooManyRequests",
                    detail = "You've sent several messages in a short time. Please wait a few minutes and try again."
                }, ct);
            };
        });

        return services;
    }
}
