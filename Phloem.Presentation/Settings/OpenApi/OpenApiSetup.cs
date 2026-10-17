using Microsoft.Extensions.Options;
using Phloem.Domain.Constants;

namespace Phloem.Presentation.Settings.OpenApi;

/// <summary>
/// Wires the API description and the Swagger page:
/// <list type="number">
///   <item><description><b>Services:</b> settings, the document and its transformers (always registered).</description></item>
///   <item><description><b>Pipeline:</b> the JSON document and the Swagger page, only when <c>OpenApi:Enabled</c> is true.</description></item>
/// </list>
/// </summary>
public static class OpenApiSetup
{
    // ───────────────────────────── 1. Services ─────────────────────────────

    /// <summary>Registers the settings, the document and its transformers.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AppSettings.OpenApi.Section);
        services.AddOptions<OpenApiSettings>().Bind(section);

        // The document name is needed now, at registration
        var settings = section.Get<OpenApiSettings>() ?? new OpenApiSettings();

        services.AddOpenApi(settings.Version, options =>
        {
            options.AddDocumentTransformer<DocumentInfoTransformer>();      // title, version, contact
            options.AddDocumentTransformer<BearerSecurityTransformer>();    // "Authorize" button
            options.AddOperationTransformer<BearerSecurityTransformer>();   // lock icons
        });

        return services;
    }

    // ───────────────────────────── 2. Pipeline ─────────────────────────────

    /// <summary>Serves the document and the Swagger page, if enabled.</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication UsePhloemOpenApi(this WebApplication app)
    {
        var settings = app.Services.GetRequiredService<IOptions<OpenApiSettings>>().Value;
        if (!settings.Enabled) return app;

        // 2a. The JSON document, e.g. /openapi/v1.json
        app.MapOpenApi(settings.DocumentPath);

        // 2b. The Swagger page, e.g. /swagger
        app.UseSwaggerUI(ui =>
        {
            ui.RoutePrefix = settings.UiPath.Trim('/');
            ui.DocumentTitle = settings.Title;
            ui.SwaggerEndpoint(settings.DocumentUrl, $"{settings.Title} {settings.Version}");
        });

        return app;
    }
}