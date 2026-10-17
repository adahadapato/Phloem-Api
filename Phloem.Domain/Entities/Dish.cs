using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A dish a shop sells. Its name always starts with an approved DishCatalog name,
/// optionally followed by a detail: "Jollof Rice" + "& Chicken" = "Jollof Rice & Chicken".
/// Its price must sit inside the admin's range for that dish in the shop's country (inclusive).
///
/// Errors: business-rule failures are RETURNED as a Result (see DishErrors);
/// programmer mistakes (nulls, a price ≤ 0 that skipped validation, an unprocessed photo) still THROW.
/// </summary>
public class Dish : Entity<Guid>
{
    /// <summary>Most photos a seller can add to one dish.</summary>
    public const int MaxPhotos = 5;

    // NEW ---------- Limits (validators in Application, column sizes in Persistence) ----------
    public const int DetailMaxLength = 40;          // "& Chicken", "Spicy"
    public const int DescriptionMaxLength = 500;

    /// <summary>Longest full name: catalog name + space + detail (e.g. "Jollof Rice" + " " + "& Chicken").</summary>
    public const int NameMaxLength = DishCatalog.NameMaxLength + 1 + DetailMaxLength;

    private readonly List<DishPhoto> _photos = new();

    protected Dish() { }

    private Dish(Guid id, Vendor vendor, DishCatalog dishCatalog, string? detail,
                 decimal price, string? description)
        : base(id)
    {
        VendorId = vendor.Id;
        Country = vendor.Country;
        Currency = vendor.Currency;
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

    /// <summary>
    /// An admin changed the price range and this dish's price is now outside it.
    /// The dish can't be ordered until the seller sets a price inside the range.
    /// </summary>
    public bool NeedsPriceUpdate { get; private set; }

    // ---------- Reputation ----------
    public decimal Rating { get; private set; }
    public int RatingCount { get; private set; }

    // ---------- Photos ----------
    public virtual IReadOnlyCollection<DishPhoto> Photos => _photos.AsReadOnly();

    /// <summary>The photo on dish cards; null until the seller adds one.</summary>
    public DishPhoto? PrimaryPhoto => _photos.FirstOrDefault(p => p.IsPrimary);

    /// <summary>Can a buyer order it right now?</summary>
    public bool CanBeOrdered => !IsDeleted && IsAvailable && !NeedsPriceUpdate && Vendor is { IsVisibleToBuyers: true };

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>Add dish page.</summary>
    public static Result<Dish> Create(Vendor vendor, DishCatalog dishCatalog, string? detail, decimal price,
                                      string? description)
    {
        ArgumentNullException.ThrowIfNull(vendor);
        ArgumentNullException.ThrowIfNull(dishCatalog);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);   // the validator catches this first

        if (vendor.Status == VendorStatus.Blocked) return DishErrors.ShopBlocked;
        if (!dishCatalog.CanBeUsed) return DishErrors.NameNotApproved;

        var priceCheck = CheckPrice(dishCatalog, vendor.Country, price);
        if (priceCheck.IsFailure) return priceCheck.Error;

        return new Dish(Guid.CreateVersion7(), vendor, dishCatalog, detail, price, description);
    }

    /// <summary>Edit dish page. A valid price here also clears NeedsPriceUpdate.</summary>
    public Result Update(DishCatalog dishCatalog, string? detail, decimal price, string? description)
    {
        ArgumentNullException.ThrowIfNull(dishCatalog);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        if (Vendor is { Status: VendorStatus.Blocked }) return DishErrors.ShopBlocked;
        if (!dishCatalog.CanBeUsed) return DishErrors.NameNotApproved;

        var priceCheck = CheckPrice(dishCatalog, Country, price);
        if (priceCheck.IsFailure) return priceCheck;

        Apply(dishCatalog, detail, price, description);
        NeedsPriceUpdate = false;
        return Result.Success();
    }

    /// <summary>
    /// Seller dashboard +/− buttons: change only the price.
    /// Uses this dish's own DishCatalog (loaded with the dish) for the range.
    /// </summary>
    public Result ChangePrice(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

        if (Vendor is { Status: VendorStatus.Blocked }) return DishErrors.ShopBlocked;

        var priceCheck = CheckPrice(DishCatalog, Country, price);
        if (priceCheck.IsFailure) return priceCheck;

        Price = RoundPrice(price);
        NeedsPriceUpdate = false;
        return Result.Success();
    }

    /// <summary>
    /// Called for each dish when an admin changes the range for its name + country.
    /// Price now outside → flagged (can't be ordered). Price inside again (range widened) → un-flagged.
    /// Returns true only if the dish was NEWLY flagged, so the seller is emailed once, not every time.
    /// </summary>
    public bool ApplyPriceRange(DishPriceRange range)
    {
        ArgumentNullException.ThrowIfNull(range);
        if (range.DishCatalogId != DishCatalogId || range.Country != Country)
            throw new InvalidOperationException("This price range is for a different dish name or country.");

        var outside = !range.Contains(Price);
        var newlyFlagged = outside && !NeedsPriceUpdate;

        NeedsPriceUpdate = outside;
        return newlyFlagged;
    }

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    /// <summary>
    /// NEW: Dish editor → Delete. Soft delete: hidden everywhere, but the row stays so past
    /// orders still point at it. Photo FILES are kept too (order history shows them).
    /// Deleting twice is fine.
    /// </summary>
    public void Delete(DateTime utcNow)
    {
        IsAvailable = false;
        SoftDelete(utcNow);
    }

    /// <summary>Stars are checked by the validator first, so out-of-range is a bug.</summary>
    public void AddRating(int stars)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stars, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stars, 5);

        Rating = Math.Round((Rating * RatingCount + stars) / (RatingCount + 1), 2);
        RatingCount++;
    }

    // ---------- Photos ----------

    /// <summary>
    /// Add a photo after the image pipeline has processed and stored it.
    /// Only standard-size photos with a thumbnail are accepted (see DishPhotoStandard), so every
    /// dish card looks the same. The first photo becomes the main one.
    /// </summary>
    public Result<DishPhoto> AddPhoto(ImageFile file, string? altText = null)
    {
        ArgumentNullException.ThrowIfNull(file);

        // The pipeline was skipped → a bug, not a user mistake
        if (file.Dimensions != DishPhotoStandard.FullSize)
            throw new InvalidOperationException(
                $"Dish photos must be processed to {DishPhotoStandard.FullSize} before they are added.");
        if (file.ThumbnailPath is null)
            throw new InvalidOperationException("Dish photos need a thumbnail.");

        // Things a seller can actually do → errors
        if (_photos.Count >= MaxPhotos) return DishErrors.TooManyPhotos;
        if (_photos.Any(p => p.File.Hash == file.Hash)) return DishErrors.DuplicatePhoto;

        var photo = new DishPhoto(Id, file, TrimOrNull(altText) ?? Name, _photos.Count);
        _photos.Add(photo);

        if (_photos.Count == 1)
            photo.MarkPrimary();

        return photo;
    }

    /// <summary>
    /// Remove a photo. The FILE stays in storage (past orders may show it).
    /// If it was the main photo, the next one takes over.
    /// </summary>
    public Result RemovePhoto(Guid photoId)
    {
        var photo = FindPhoto(photoId);
        if (photo is null) return DishErrors.PhotoNotFound;

        _photos.Remove(photo);
        Renumber(_photos.OrderBy(p => p.DisplayOrder).ToList());

        if (photo.IsPrimary && _photos.Count > 0)
            _photos.OrderBy(p => p.DisplayOrder).First().MarkPrimary();

        return Result.Success();
    }

    public Result SetPrimaryPhoto(Guid photoId)
    {
        var chosen = FindPhoto(photoId);
        if (chosen is null) return DishErrors.PhotoNotFound;

        foreach (var p in _photos) p.ClearPrimary();
        chosen.MarkPrimary();
        return Result.Success();
    }

    /// <summary>Drag-to-reorder on the Edit dish page: pass every photo Id in the new order.</summary>
    public Result ReorderPhotos(IReadOnlyList<Guid> photoIdsInOrder)
    {
        ArgumentNullException.ThrowIfNull(photoIdsInOrder);

        var sameSet = photoIdsInOrder.Count == _photos.Count
                      && photoIdsInOrder.Distinct().Count() == _photos.Count
                      && photoIdsInOrder.All(id => _photos.Any(p => p.Id == id));
        if (!sameSet) return DishErrors.PhotoOrderMismatch;

        Renumber(photoIdsInOrder.Select(id => _photos.First(p => p.Id == id)).ToList());
        return Result.Success();
    }

    public Result UpdatePhotoAltText(Guid photoId, string altText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(altText);

        var photo = FindPhoto(photoId);
        if (photo is null) return DishErrors.PhotoNotFound;

        photo.UpdateAltText(altText.Trim());
        return Result.Success();
    }

    // ---------- Helpers ----------

    /// <summary>The one place the price rule lives (range must exist, price inside it, inclusive).</summary>
    private static Result CheckPrice(DishCatalog dishCatalog, Country country, decimal price)
    {
        var range = dishCatalog.PriceRangeFor(country);
        if (range is null) return DishErrors.PriceNotSet;

        return range.Contains(RoundPrice(price))
            ? Result.Success()
            : DishErrors.PriceOutOfRange(range);
    }

    private static decimal RoundPrice(decimal price) =>
        decimal.Round(price, 2, MidpointRounding.AwayFromZero);

    private DishPhoto? FindPhoto(Guid photoId) => _photos.FirstOrDefault(p => p.Id == photoId);

    private static void Renumber(IList<DishPhoto> ordered)
    {
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SetDisplayOrder(i);
    }

    /// <summary>Sets name, category, price and description. Callers have already checked the rules.</summary>
    private void Apply(DishCatalog dishCatalog, string? detail, decimal price, string? description)
    {
        var cleanDetail = TrimOrNull(detail);
        var cleanDescription = TrimOrNull(description);

        // NEW: the validator checks these first, so getting here too long is a bug
        if (cleanDetail is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cleanDetail.Length, DetailMaxLength, nameof(detail));
        if (cleanDescription is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cleanDescription.Length, DescriptionMaxLength, nameof(description));

        DishCatalogId = dishCatalog.Id;
        Category = dishCatalog.Category;
        Detail = cleanDetail;
        Name = Detail is null ? dishCatalog.Name : $"{dishCatalog.Name} {Detail}";
        Price = RoundPrice(price);
        Description = cleanDescription;
    }

    /// <summary>Blank text → null, otherwise trimmed.</summary>
    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}