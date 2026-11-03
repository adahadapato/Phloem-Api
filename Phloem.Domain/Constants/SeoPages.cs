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
            "Phloem's home page. Sections, top to bottom: a search box for dishes and shops; \"What are you " +
            "craving?\" (dish types to tap); dishes picked for the visitor; popular dishes near them, with photos " +
            "and prices; \"Meet the cooks\" (home cooks and food businesses with ratings); \"How Phloem works\" " +
            "(choose, order and pay, get it delivered); \"Send a taste of home\" (parcels between the UK, Ghana and " +
            "Nigeria); \"Cook for Phloem\" (how to open a shop). Links to Explore, Shops, Send a parcel, Track and " +
            "Help. Food is from home cooks and food businesses in the United Kingdom, Ghana and Nigeria.",
            "Phloem: African home-cooked food and parcels",
            "Order African home-cooked food from local cooks in the UK, Ghana and Nigeria, and send parcels between them with Phloem."),
        new("explore", "Explore dishes", "/explore",
            "Explore. Heading: \"African dishes to order in <country>\", with an introduction naming popular " +
            "dishes (jollof rice, waakye, egusi soup, suya), who cooks them, and delivery today or tomorrow. Below: " +
            "every African dish on sale in the visitor's country (United Kingdom, Ghana or Nigeria), as " +
            "cards with a photo, price, rating and the shop's name. Search box with suggestions as you type, " +
            "filters by dish type (e.g. rice dishes, soups and stews, snacks), sort, and pages. Each dish can be " +
            "added to the basket and ordered for delivery today or tomorrow.",
            "Explore African dishes near you | Phloem",
            "Browse jollof rice, waakye, egusi soup and more from home cooks near you. See prices and photos, and order for delivery."),
        new("shops", "Shops", "/shops",
            "Shops. Heading: \"African food shops in <country>\", with an introduction explaining the ✓ Verified " +
            "badge. Below: every home cook and food business selling on Phloem in the visitor's country, as cards with " +
            "logo, name, ✓ Verified badge, star rating and number of dishes, with a search box. Each card opens " +
            "the shop's own page with its full menu.",
            "African food shops and home cooks | Phloem",
            "Find home cooks and food businesses selling African food near you, with ratings, dishes and delivery."),
        new("send-parcel", "Send a parcel", "/send-parcel",
            "Send a parcel. An introduction under the heading explains booking online, the price estimate, " +
            "drop-off and weighing at the hub, and tracking to the door. The page lets visitors book a parcel between the United Kingdom, Ghana and Nigeria. Steps on the page: choose " +
            "the route and speed, list the items and their value, get a price estimate, add the recipient, then " +
            "drop the parcel at Phloem's hub where it is weighed and the final price set. Shows what can and " +
            "can't be sent, and every parcel gets a tracking number.",
            "Send a parcel to Ghana, Nigeria or the UK | Phloem",
            "Book a parcel between the UK, Ghana and Nigeria. Get a price estimate, check what can be sent and track it all the way."),
        new("track", "Track a parcel", "/track",
            "Track a parcel. An introduction under the heading explains the tracking number format and the steps " +
            "shown. Visitors enter a Phloem tracking number (e.g. PHS-…) to see every step of the parcel's journey: " +
            "booked, received and weighed at the hub, paid, in transit, arrived, out for delivery, delivered with " +
            "a photo. Also links to sending a parcel.",
            "Track your parcel | Phloem",
            "Enter your Phloem tracking number to see where your parcel is, from drop-off to delivery."),
        new("help", "Help", "/help",
            "Help: answers to common questions, in sections: ordering food and paying, delivery times, cancelling " +
            "and refunds, selling on Phloem (opening a shop, documents, earnings), couriers, and sending and " +
            "tracking parcels. Links to Contact us.",
            "Help and frequently asked questions | Phloem",
            "Answers about ordering food, selling on Phloem, delivery times and sending parcels."),
        new("contact", "Contact us", "/contact",
            "Contact us: a message form (with topics such as an order, a parcel or selling), Phloem's phone, " +
            "WhatsApp, address and opening hours, how soon the team replies, and quick answers to common " +
            "questions with links to Help.",
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
