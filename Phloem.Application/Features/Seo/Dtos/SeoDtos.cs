using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Seo.Dtos;

/// <summary>Admin → SEO, "Website health": what search engines can see and how much is written.</summary>
/// <param name="AiIsOn">The AI can write text now.</param>
/// <param name="AiNotReadyReason">Why not, when it can't.</param>
/// <param name="MainPages">Fixed pages (Home, Explore…).</param>
/// <param name="MainPagesWritten">Of those, with text.</param>
/// <param name="Shops">Live shops.</param>
/// <param name="ShopsWritten">Of those, with text.</param>
/// <param name="ShopsByStaff">Of those, written or changed by staff.</param>
/// <param name="PagesChanged">AI-written pages whose page changed since (rewritten by themselves soon).</param>
/// <param name="SitemapUrl">The sitemap Google reads.</param>
/// <param name="RobotsUrl">The website's robots.txt.</param>
/// <param name="PagesInSitemap">Addresses in the sitemap.</param>
public sealed record SeoOverviewDto(
    bool AiIsOn,
    string? AiNotReadyReason,
    int MainPages,
    int MainPagesWritten,
    int Shops,
    int ShopsWritten,
    int ShopsByStaff,
    int PagesChanged,
    string SitemapUrl,
    string RobotsUrl,
    int PagesInSitemap);

/// <summary>One page in the Admin → SEO list.</summary>
/// <param name="Key">"home", "shop-…".</param>
/// <param name="Kind">Fixed page or shop.</param>
/// <param name="Name">Page or shop name.</param>
/// <param name="Path">Website address.</param>
/// <param name="Country">The shop's country (null for fixed pages).</param>
/// <param name="Status">Not written / by AI / by staff.</param>
/// <param name="PageChanged">Written by the AI before the page changed (it rewrites it soon).</param>
/// <param name="Title">The title Google gets now.</param>
/// <param name="Score">The AI's score.</param>
public sealed record SeoPageRowDto(
    string Key,
    SeoPageKind Kind,
    string Name,
    string Path,
    Country? Country,
    SeoPageStatus Status,
    bool PageChanged,
    string Title,
    int? Score);

/// <summary>One page on Admin → SEO: the text, the Google preview, the AI's tips and score.</summary>
/// <param name="Key">The page key.</param>
/// <param name="Kind">Fixed page or shop.</param>
/// <param name="Name">Page or shop name.</param>
/// <param name="Path">Website address.</param>
/// <param name="Url">Full website address (for the preview).</param>
/// <param name="ShareUrl">The link that shows a preview on WhatsApp / Facebook (shops only).</param>
/// <param name="Country">The shop's country.</param>
/// <param name="Status">Not written / by AI / by staff.</param>
/// <param name="PageChanged">Written by the AI before the page changed.</param>
/// <param name="Title">The title Google gets now (Phloem's default when not written).</param>
/// <param name="Description">The description Google gets now.</param>
/// <param name="Keywords">Search phrases.</param>
/// <param name="Improvements">The AI's tips.</param>
/// <param name="Score">The AI's score, 0–100 (an estimate, not a Google score).</param>
/// <param name="ScoreExplanation">Why that score.</param>
/// <param name="WhatTheAiGets">In plain words, what the AI is sent for this page.</param>
/// <param name="ChangedBy">Who last wrote or asked for it.</param>
/// <param name="ChangedAt">When.</param>
public sealed record SeoPageDetailsDto(
    string Key,
    SeoPageKind Kind,
    string Name,
    string Path,
    string Url,
    string? ShareUrl,
    Country? Country,
    SeoPageStatus Status,
    bool PageChanged,
    string Title,
    string Description,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<string> Improvements,
    int? Score,
    string? ScoreExplanation,
    string WhatTheAiGets,
    string? ChangedBy,
    DateTime? ChangedAt);

/// <summary>What the website puts in a public page's head (title, description, preview picture, details for Google).</summary>
/// <param name="Title">The page title.</param>
/// <param name="Description">The description.</param>
/// <param name="Keywords">Search phrases (comma-separated).</param>
/// <param name="ImageUrl">Preview picture (shops: cover or logo); null = the site's default.</param>
/// <param name="CanonicalUrl">The page's one true address.</param>
/// <param name="JsonLd">Details for Google (schema.org JSON), shops only.</param>
public sealed record PublicSeoDto(
    string Title,
    string Description,
    string Keywords,
    string? ImageUrl,
    string CanonicalUrl,
    string? JsonLd);

/// <summary>One address in the sitemap.</summary>
/// <param name="Path">Website address, e.g. "/shops/…".</param>
/// <param name="ChangedAtUtc">When it last changed; null for fixed pages.</param>
public sealed record SitemapUrlDto(string Path, DateTime? ChangedAtUtc);
