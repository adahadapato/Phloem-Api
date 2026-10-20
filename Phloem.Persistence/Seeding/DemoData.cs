using Phloem.Domain.Enums;

namespace Phloem.Persistence.Seeding;

/// <summary>The contents of <c>demo-data.json</c>: demo shops with their owners, pictures and dishes.</summary>
internal sealed class DemoData
{
    /// <summary>The demo shops.</summary>
    public List<DemoShop> Shops { get; set; } = [];
}

/// <summary>One demo shop, its owner, pictures and dishes.</summary>
internal sealed class DemoShop
{
    public DemoOwner Owner { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string BusinessEmail { get; set; } = string.Empty;

    /// <summary>The shop's delivery fee; must be inside its country's range (starter data).</summary>
    public decimal DeliveryFee { get; set; }

    public DemoPicture Logo { get; set; } = new();
    public DemoPicture CoverPhoto { get; set; } = new();

    /// <summary>Star ratings (1–5) given to the shop, so it has a rating to show.</summary>
    public List<int> Ratings { get; set; } = [];

    public List<DemoDish> Dishes { get; set; } = [];
}

/// <summary>The shop's owner: a real (demo) account with personal details, so the shop passes the same rules.</summary>
internal sealed class DemoOwner
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    /// <summary>The demo account's sign-in email; also how the seeder knows the shop is already there.</summary>
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public Country Country { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DemoAddress Address { get; set; } = new();
}

/// <summary>The owner's home address (country = the owner's country).</summary>
internal sealed class DemoAddress
{
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string RegionOrPostcode { get; set; } = string.Empty;
}

/// <summary>A ready-made picture in <c>demo-photos/</c> (already WebP, already the right size).</summary>
internal sealed class DemoPicture
{
    /// <summary>Path inside <c>demo-photos/</c>, e.g. "shops/lagos-flavours-logo.webp".</summary>
    public string File { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>One dish on a demo shop's menu.</summary>
internal sealed class DemoDish
{
    /// <summary>A name from the dish catalog, e.g. "Jollof Rice".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional detail after the name, e.g. "&amp; Chicken".</summary>
    public string? Detail { get; set; }

    /// <summary>Must be inside the catalog's range for the shop's country.</summary>
    public decimal Price { get; set; }

    public string? Description { get; set; }

    /// <summary>Photo in <c>demo-photos/</c> without extension: "{photo}.webp" (1200×900) and "{photo}_thumb.webp".</summary>
    public string Photo { get; set; } = string.Empty;

    public List<int> Ratings { get; set; } = [];
}
