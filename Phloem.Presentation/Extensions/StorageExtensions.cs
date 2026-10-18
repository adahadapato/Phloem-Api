using Microsoft.AspNetCore.StaticFiles;
using Phloem.Infrastructure.Settings.Storage;

namespace Phloem.Presentation.Extensions;

/// <summary>
/// Serving stored files:
/// <list type="number">
///   <item><description><b>Public files</b> (photos, logos) from <c>wwwroot</c>, e.g. <c>/uploads/…</c>.</description></item>
///   <item><description><b>The private folder</b> is blocked (404) if it sits inside <c>wwwroot</c>.</description></item>
///   <item><description><b>Private files</b> (documents) only through signed, expiring links, e.g.
///   <c>/private-files/…?expires=…&amp;sig=…</c>.</description></item>
/// </list>
/// </summary>
/// <remarks>Paths come from <see cref="LocalStoragePaths"/> (settings), never from code. When another storage provider
/// is used, it serves its own files and both methods do nothing.</remarks>
public static class StorageExtensions
{
    /// <summary>Works out a file's content type from its extension.</summary>
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    /// <summary>Blocks the private folder, then serves <c>wwwroot</c>. Call before authentication.</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication UsePhloemStaticFiles(this WebApplication app)
    {
        var paths = app.Services.GetService<LocalStoragePaths>();
        if (paths is null) return app;

        // 1. Never serve the private folder directly, even though it's inside wwwroot
        if (paths.BlockedRequestPath is { } blocked)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(blocked, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                await next(context);
            });
        }

        // 2. Public files (photos, logos)
        app.UseStaticFiles();
        return app;
    }

    /// <summary>Maps the signed download endpoint for private files.</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication MapPrivateFiles(this WebApplication app)
    {
        var paths = app.Services.GetService<LocalStoragePaths>();
        if (paths is null) return app;

        app.MapGet($"{paths.PrivateRequestPath}/{{**path}}",
                (string path, long expires, string? sig, PrivateFileLinks links, HttpContext context) =>
                {
                    // A wrong, expired or tampered link looks exactly like a missing file
                    if (!links.TryResolve(path, expires, sig, out var fullPath))
                        return Results.NotFound();

                    if (!ContentTypes.TryGetContentType(fullPath, out var contentType))
                        contentType = "application/octet-stream";

                    context.Response.Headers.CacheControl = "private, no-store";
                    return Results.File(fullPath, contentType, enableRangeProcessing: true);
                })
            .AllowAnonymous()             // the signature IS the permission
            .ExcludeFromDescription();

        return app;
    }
}
