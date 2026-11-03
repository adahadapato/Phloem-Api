using System.Net;
using System.Text;
using MediatR;
using Phloem.Application.Features.Seo.Commands;
using Phloem.Application.Features.Seo.Dtos;
using Phloem.Application.Features.Seo.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authorization;

namespace Phloem.Presentation.Endpoints.Seo;

/// <summary>
/// Search engine text (SEO):
/// <list type="bullet">
///   <item><description><b>Admin</b> (<c>/api/admin/seo</c>, permission <c>seo.manage</c>): website health, the pages,
///   edit a page's text, "Write with AI", "Write missing pages now".</description></item>
///   <item><description><b>Website</b> (<c>/api/seo?path=…</c>, anyone): the title, description and details for one
///   public page.</description></item>
///   <item><description><b>Search engines and chat apps</b> (outside <c>/api</c>, anyone): <c>/sitemap.xml</c>, and
///   <c>/share/shops/{id}</c>, a small page with preview tags (WhatsApp, Facebook) that sends people on to the
///   shop.</description></item>
/// </list>
/// </summary>
public static class SeoEndpoints
{
    /// <summary>Maps the admin and website SEO endpoints under <c>/api</c>.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder api)
    {
        // ───────────── Website ─────────────
        api.MapGet("/seo", async (string path, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetPublicSeoQuery(path), ct)).ToHttp())
            .WithTags("SEO")
            .WithName("GetPublicSeo")
            .WithSummary("Title, description, preview picture and Google details for one public page")
            .AllowAnonymous()
            .Produces<PublicSeoDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────── Admin ─────────────
        var admin = api.MapGroup("/admin/seo")
                       .WithTags("Admin: SEO")
                       .RequirePermission(Permissions.Content.Seo, CountryRule.AllCountries);

        admin.MapGet("/overview", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetSeoOverviewQuery(), ct)).ToHttp())
            .WithName("GetSeoOverview")
            .WithSummary("Website health: how many pages have text, sitemap and robots links")
            .Produces<SeoOverviewDto>();

        admin.MapGet("/pages", async ([AsParameters] GetSeoPagesQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetSeoPages")
            .WithSummary("The pages (fixed pages and live shops), with filters")
            .Produces<PagedResult<SeoPageRowDto>>()
            .ProducesValidationProblem();

        admin.MapGet("/pages/{key}", async (string key, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetSeoPageQuery(key), ct)).ToHttp())
            .WithName("GetSeoPage")
            .WithSummary("One page: its text, the AI's tips and score")
            .Produces<SeoPageDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPut("/pages/{key}", async (string key, SaveSeoRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new SaveSeoPageCommand(key, body.Title, body.Description, body.Keywords), ct))
                    .ToHttp())
            .WithName("SaveSeoPage")
            .WithSummary("Save staff's own title, description and keywords (the AI then leaves them alone)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPost("/pages/{key}/write", async (string key, ISender sender, CancellationToken ct) =>
                (await sender.Send(new WriteSeoPageCommand(key), ct)).ToHttp())
            .WithName("WriteSeoPage")
            .WithSummary("The AI writes this page's text now")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPost("/write-missing", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new WriteMissingSeoCommand(), ct)).ToHttp())
            .WithName("WriteMissingSeo")
            .WithSummary("Write up to 25 pages that need it now (instead of waiting for the half-hourly run)")
            .Produces<int>();

        return api;
    }

    /// <summary>Maps <c>/sitemap.xml</c> and <c>/share/shops/{id}</c> (outside <c>/api</c>, not in Swagger).</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication MapSitemapAndShareLinks(this WebApplication app)
    {
        app.MapGet("/sitemap.xml", async (ISender sender, ISiteAddresses site, CancellationToken ct) =>
            {
                var urls = await sender.Send(new GetSitemapQuery(), ct);
                var xml = new StringBuilder("""<?xml version="1.0" encoding="UTF-8"?>""").Append('\n')
                    .Append("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""").Append('\n');
                foreach (var url in urls.IsSuccess ? urls.Value : Array.Empty<SitemapUrlDto>())
                {
                    xml.Append("  <url><loc>").Append(WebUtility.HtmlEncode(site.Website + url.Path)).Append("</loc>");
                    if (url.ChangedAtUtc is { } changed) xml.Append("<lastmod>").Append(changed.ToString("yyyy-MM-dd")).Append("</lastmod>");
                    xml.Append("</url>\n");
                }
                xml.Append("</urlset>\n");
                return Results.Text(xml.ToString(), "application/xml", Encoding.UTF8);
            })
            .AllowAnonymous()
            .ExcludeFromDescription();

        app.MapGet("/share/shops/{id:guid}", async (Guid id, ISender sender, ISiteAddresses site, CancellationToken ct) =>
            {
                var target = site.Website + SeoPages.ShopPath(id);
                var seo = await sender.Send(new GetPublicSeoQuery(SeoPages.ShopPath(id)), ct);
                if (seo.IsFailure) return Results.Redirect(target);

                var s = seo.Value;
                string E(string? v) => WebUtility.HtmlEncode(v ?? string.Empty);
                var image = s.ImageUrl is null ? "" : $"""<meta property="og:image" content="{E(s.ImageUrl)}">""";
                var html = $"""
                    <!doctype html>
                    <html lang="en-GB">
                    <head>
                    <meta charset="utf-8">
                    <title>{E(s.Title)}</title>
                    <meta name="description" content="{E(s.Description)}">
                    <meta property="og:type" content="website">
                    <meta property="og:site_name" content="Phloem">
                    <meta property="og:title" content="{E(s.Title)}">
                    <meta property="og:description" content="{E(s.Description)}">
                    <meta property="og:url" content="{E(s.CanonicalUrl)}">
                    {image}
                    <meta name="twitter:card" content="summary_large_image">
                    <link rel="canonical" href="{E(s.CanonicalUrl)}">
                    <meta http-equiv="refresh" content="0; url={E(target)}">
                    </head>
                    <body><p><a href="{E(target)}">Open {E(s.Title)} on Phloem</a></p></body>
                    </html>
                    """;
                return Results.Content(html, "text/html", Encoding.UTF8);
            })
            .AllowAnonymous()
            .ExcludeFromDescription();

        return app;
    }

    /// <summary>Body of "save a page's text".</summary>
    /// <param name="Title">The title Google shows.</param>
    /// <param name="Description">The description Google shows.</param>
    /// <param name="Keywords">Search phrases.</param>
    public sealed record SaveSeoRequest(string Title, string Description, IReadOnlyList<string>? Keywords);
}
