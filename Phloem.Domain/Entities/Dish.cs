using Phloem.Domain.Constants;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A dish a shop sells. Its name always starts with an approved DishCatalog name,
/// optionally followed by a detail: "Jollof Rice" + "& Chicken" = "Jollof Rice & Chicken".
/// </summary>
public class Dish : Entity<Guid>
{
    protected Dish() { }

    private Dish(Guid id, Vendor vendor, DishCatalog dishCatalog, string? detail,
                 decimal price, string? description, string? imageUrl, DateTime createdAtUtc)
        : base(id)
    {
        VendorId = vendor.Id;
        DishCatalogId = dishCatalog.Id;
        Country = vendor.Country;
        Currency = vendor.Currency;
        Category = dishCatalog.Category;
        CreatedAtUtc = createdAtUtc;
        IsAvailable = true;
        Apply(dishCatalog, detail, price, description, imageUrl);
    }

    // ---------- Who sells it ----------
    public Guid VendorId { get; private set; }
    public virtual Vendor Vendor { get; protected set; } = null!;

    // ---------- Name (from the dish list) ----------
    public Guid DishCatalogId { get; private set; }
    public virtual DishCatalog DishCatalog { get; protected set; } = null!;
    public string? Detail { get; private set; }                     // "& Chicken", "Spicy"
    public string Name { get; private set; } = string.Empty;        // full name shown to buyers
    public DishCategory Category { get; private set; }

    // ---------- Selling ----------
    public decimal Price { get; private set; }
    public Currency Currency { get; private set; }                  // same as the shop's
    public Country Country { get; private set; }                    // same as the shop's
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }                   // set after the photo upload
    public bool IsAvailable { get; private set; }                   // "Available today" switch

    // ---------- Reputation ----------
    public decimal Rating { get; private set; }
    public int RatingCount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    // =====================================================================
    // Behaviour
    // =====================================================================

    public static Dish Create(Vendor vendor, DishCatalog dishCatalog, string? detail, decimal price,
                              string? description, string? imageUrl, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(vendor);
        if (vendor.Status == VendorStatus.Blocked)
            throw new InvalidOperationException("This shop is blocked and can't add dishes.");

        return new Dish(Guid.CreateVersion7(), vendor, dishCatalog, detail, price, description, imageUrl, utcNow);
    }

    /// <summary>Edit dish page.</summary>
    public void Update(DishCatalog dishCatalog, string? detail, decimal price, string? description)
    {
        Apply(dishCatalog, detail, price, description, ImageUrl);
        DishCatalogId = dishCatalog.Id;
        Category = dishCatalog.Category;
    }

    public void ChangePhoto(string? imageUrl) =>
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    public void AddRating(int stars)
    {
        if (stars is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(stars), "Rating must be 1 to 5 stars.");
        Rating = Math.Round((Rating * RatingCount + stars) / (RatingCount + 1), 2);
        RatingCount++;
    }

    /// <summary>Can a buyer order it right now?</summary>
    public bool CanBeOrdered => IsAvailable && Vendor is { IsVisibleToBuyers: true };

    private void Apply(DishCatalog dishCatalog, string? detail, decimal price, string? description, string? imageUrl)
    {
        ArgumentNullException.ThrowIfNull(dishCatalog);
        if (!dishCatalog.CanBeUsed)
            throw new InvalidOperationException("Please choose a dish name from the approved list.");
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be more than zero.");

        Detail = Rules.Optional(detail, "Detail", 40);
        Name = Detail is null ? dishCatalog.Name : $"{dishCatalog.Name} {Detail}";
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        Description = Rules.Optional(description, "Description", 500);
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }
}