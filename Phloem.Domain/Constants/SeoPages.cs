using Phloem.Domain.Models;

namespace Phloem.Domain.Constants;

/// <summary>
/// The website pages that have search engine text (SEO): the fixed pages below, plus one per live shop
/// (key <c>shop-{id}</c>, address <c>/shops/{id}</c>).
/// </summary>
/// <remarks>
/// Pages people must be signed in for (checkout, orders, seller, admin) are never listed: search engines are told to
/// skip them (robots.txt) and they're not in the sitemap.
/// </remarks>
public static class SeoPages
{
    /// <summary>Start of every shop page's key.</summary>
    public const string ShopKeyPrefix = "shop-";

    /// <summary>Longest search title kept.</summary>
    public const int TitleMaxLength = 70;

    /// <summary>Longest search description kept.</summary>
    public const int DescriptionMaxLength = 170;

    /// <summary>The fixed pages, in the order Admin → SEO lists them.</summary>
    public static IReadOnlyList<SeoPageDefinition> Main { get; } =
    [
        new("home", "Home", "/",
            "Phloem's home page: order African home-cooked food from home cooks and food businesses in the United " +
            "Kingdom, Ghana and Nigeria, and send parcels between these countries.",
            "Phloem: African home-cooked food and parcels",
            "Order African home-cooked food from local cooks in the UK, Ghana and Nigeria, and send parcels between them with Phloem."),
        new("explore", "Explore dishes", "/explore",
            "Every dish on sale in the visitor's country, with photos and prices, to order for delivery.",
            "Explore African dishes near you | Phloem",
            "Browse jollof rice, waakye, egusi soup and more from home cooks near you. See prices and photos, and order for delivery."),
        new("shops", "Shops", "/shops",
            "Every home cook and food business selling on Phloem in the visitor's country.",
            "African food shops and home cooks | Phloem",
            "Find home cooks and food businesses selling African food near you, with ratings, dishes and delivery."),
        new("send-parcel", "Send a parcel", "/send-parcel",
            "Book a parcel between the United Kingdom, Ghana and Nigeria: price estimate, what can be sent, drop-off at the hub.",
            "Send a parcel to Ghana, Nigeria or the UK | Phloem",
            "Book a parcel between the UK, Ghana and Nigeria. Get a price estimate, check what can be sent and track it all the way."),
        new("track", "Track a parcel", "/track",
            "Track a Phloem parcel with its tracking number and see every step of its journey.",
            "Track your parcel | Phloem",
            "Enter your Phloem tracking number to see where your parcel is, from drop-off to delivery."),
        new("help", "Help", "/help",
            "Answers to common questions about ordering food, selling, delivery and parcels on Phloem.",
            "Help and frequently asked questions | Phloem",
            "Answers about ordering food, selling on Phloem, delivery times and sending parcels."),
        new("contact", "Contact us", "/contact",
            "How to contact Phloem's team: form, phone, WhatsApp and opening hours.",
            "Contact Phloem",
            "Questions about an order, a parcel or selling on Phloem? Send us a message and we'll reply quickly.")
    ];

    /// <summary>A fixed page by key, or null.</summary>
    /// <param name="key">The key, e.g. "home".</param>
    public static SeoPageDefinition? FindMain(string key) =>
        Main.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.Ordinal));

    /// <summary>The fixed page at a website address, or null.</summary>
    /// <param name="path">The address, e.g. "/explore".</param>
    public static SeoPageDefinition? FindMainByPath(string path) =>
        Main.FirstOrDefault(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase));

    /// <summary>A shop page's key.</summary>
    /// <param name="shopId">The shop.</param>
    public static string ShopKey(Guid shopId) => $"{ShopKeyPrefix}{shopId:D}";

    /// <summary>A shop page's website address.</summary>
    /// <param name="shopId">The shop.</param>
    public static string ShopPath(Guid shopId) => $"/shops/{shopId:D}";

    /// <summary>The shop in a key like "shop-…", or null.</summary>
    /// <param name="key">The key.</param>
    public static Guid? ShopIdFromKey(string key) =>
        key.StartsWith(ShopKeyPrefix, StringComparison.Ordinal)
        && Guid.TryParse(key[ShopKeyPrefix.Length..], out var id) ? id : null;

    /// <summary>The shop in an address like "/shops/…", or null.</summary>
    /// <param name="path">The address.</param>
    public static Guid? ShopIdFromPath(string path)
    {
        var p = path.Trim().TrimEnd('/');
        return p.StartsWith("/shops/", StringComparison.OrdinalIgnoreCase)
               && Guid.TryParse(p["/shops/".Length..], out var id) ? id : null;
    }
}
