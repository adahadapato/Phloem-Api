namespace Phloem.Presentation.Extensions;

/// <summary>
/// Maps every endpoint, so Program.cs has one line. Feature groups live under <c>/api</c>; <c>/health</c> sits outside
/// it for the host's uptime checks.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>Maps all endpoints.</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication MapPhloemEndpoints(this WebApplication app)
    {
        // 1. API groups
        var api = app.MapGroup("/api");
        //api.MapAuthEndpoints();      // /api/auth
                                     // next: /api/me, /api/shops, /api/dishes, /api/orders, /api/shipments, /api/admin…

        // 2. Health (not in Swagger)
        app.MapGet("/health",
                () => Results.Ok(
                    new
                    {
                        status = "healthy",
                        timestamp = DateTime.UtcNow
                    }))
            .ExcludeFromDescription();

        return app;
    }
}
