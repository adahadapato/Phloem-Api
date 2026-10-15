using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A dish a shop sells. Its name always starts with an approved DishCatalog name,
/// optionally followed by a detail: "Jollof Rice" + "& Chicken" = "Jollof Rice & Chicken".
/// </summary>
public class Dish : Entity<Guid>
{
    /// <summary>Most photos a seller can add to one dish.</summary>
    public const int MaxPhotos = 5;

    private readonly List<DishPhoto> _photos = new();

    protected Dish() { }

    private Dish(Guid id, Vendor vendor, DishCatalog dishCatalog, string? detail,
                 decimal price, string? description)
        : base(id)
    {
        VendorId = vendor.Id;
        DishCatalogId = dishCatalog.Id;
        Country = vendor.Country;
        Currency = vendor.Currency;
        Category = dishCatalog.Category;
        IsAvailable = true;
        Apply(dishCatalog, detail, price, description);
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
    public bool IsAvailable { get; private set; }                   // "Available today" switch

    // ---------- Reputation ----------
    public decimal Rating { get; private set; }
    public int RatingCount { get; private set; }

    // ---------- Photos ----------
    public virtual IReadOnlyCollection<DishPhoto> Photos => _photos.AsReadOnly();

    /// <summary>The photo on dish cards; null until the seller adds one.</summary>
    public DishPhoto? PrimaryPhoto => _photos.FirstOrDefault(p => p.IsPrimary);

    // =====================================================================
    // Behaviour
    // =====================================================================

    public static Dish Create(Vendor vendor, DishCatalog dishCatalog, string? detail, decimal price,
                              string? description)
    {
        ArgumentNullException.ThrowIfNull(vendor);
        if (vendor.Status == VendorStatus.Blocked)
            throw new InvalidOperationException("This shop is blocked and can't add dishes.");

        return new Dish(Guid.CreateVersion7(), vendor, dishCatalog, detail, price, description);
    }

    /// <summary>Edit dish page.</summary>
    public void Update(DishCatalog dishCatalog, string? detail, decimal price, string? description)
    {
        Apply(dishCatalog, detail, price, description);
        DishCatalogId = dishCatalog.Id;
        Category = dishCatalog.Category;
    }

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    public void AddRating(int stars)
    {
        if (stars is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(stars), "Rating must be 1 to 5 stars.");
        Rating = Math.Round((Rating * RatingCount + stars) / (RatingCount + 1), 2);
        RatingCount++;
    }

    /// <summary>Can a buyer order it right now?</summary>
    public bool CanBeOrdered => IsAvailable && Vendor is { IsVisibleToBuyers: true };

    // ---------- Photos ----------

    /// <summary>
    /// Add a photo after the image pipeline has processed and stored it.
    /// Only standard-size photos with a thumbnail are accepted (see DishPhotoStandard), so every
    /// dish card looks the same. The first photo becomes the main one; duplicates are refused.
    /// </summary>
    public DishPhoto AddPhoto(ImageFile file, string? altText = null)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Dimensions != DishPhotoStandard.FullSize)
            throw new InvalidOperationException(
                $"Dish photos must be processed to {DishPhotoStandard.FullSize} before they are added.");
        if (file.ThumbnailPath is null)
            throw new InvalidOperationException("Dish photos need a thumbnail.");

        if (_photos.Count >= MaxPhotos)
            throw new InvalidOperationException($"A dish can have at most {MaxPhotos} photos.");
        if (_photos.Any(p => p.File.Hash == file.Hash))
            throw new InvalidOperationException("You've already added this photo.");

        var photo = new DishPhoto(Id, file, TrimOrNull(altText) ?? Name, _photos.Count);
        _photos.Add(photo);

        if (_photos.Count == 1)
            photo.MarkPrimary();

        return photo;
    }

    /// <summary>
    /// Remove a photo. The caller (Application layer) deletes the file from storage afterwards.
    /// If it was the main photo, the next one takes over.
    /// </summary>
    public void RemovePhoto(Guid photoId)
    {
        var photo = FindPhoto(photoId);
        _photos.Remove(photo);

        Renumber(_photos.OrderBy(p => p.DisplayOrder).ToList());

        if (photo.IsPrimary && _photos.Count > 0)
            _photos.OrderBy(p => p.DisplayOrder).First().MarkPrimary();
    }

    public void SetPrimaryPhoto(Guid photoId)
    {
        var chosen = FindPhoto(photoId);
        foreach (var p in _photos) p.ClearPrimary();
        chosen.MarkPrimary();
    }

    /// <summary>Drag-to-reorder on the Edit dish page: pass every photo Id in the new order.</summary>
    public void ReorderPhotos(IReadOnlyList<Guid> photoIdsInOrder)
    {
        ArgumentNullException.ThrowIfNull(photoIdsInOrder);
        if (photoIdsInOrder.Count != _photos.Count || photoIdsInOrder.Distinct().Count() != _photos.Count)
            throw new ArgumentException("Send every photo exactly once.", nameof(photoIdsInOrder));

        Renumber(photoIdsInOrder.Select(FindPhoto).ToList());
    }

    public void UpdatePhotoAltText(Guid photoId, string altText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(altText);
        FindPhoto(photoId).UpdateAltText(altText.Trim());
    }

    private DishPhoto FindPhoto(Guid photoId) =>
        _photos.FirstOrDefault(p => p.Id == photoId)
        ?? throw new InvalidOperationException("That photo wasn't found on this dish.");

    private static void Renumber(IList<DishPhoto> ordered)
    {
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SetDisplayOrder(i);
    }

    private void Apply(DishCatalog dishCatalog, string? detail, decimal price, string? description)
    {
        ArgumentNullException.ThrowIfNull(dishCatalog);
        if (!dishCatalog.CanBeUsed)
            throw new InvalidOperationException("Please choose a dish name from the approved list.");
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be more than zero.");

        Detail = TrimOrNull(detail);
        Name = Detail is null ? dishCatalog.Name : $"{dishCatalog.Name} {Detail}";
        Price = decimal.Round(price, 2, MidpointRounding.AwayFromZero);
        Description = TrimOrNull(description);
    }

    /// <summary>Blank text → null, otherwise trimmed.</summary>
    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}