using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// One photo of a dish. The image file lives in storage; this row records where it is, its details, and how it is
/// shown (alt text, order, which one is the main photo). Belongs to a Dish and is only created / changed through it.
/// </summary>
public class DishPhoto : Entity<Guid>
{
    /// <summary>Longest alt text (column size). Defaults to the dish name, which always fits.</summary>
    public const int AltTextMaxLength = 150;

    /// <summary>Required by EF Core.</summary>
    protected DishPhoto() { }

    /// <summary>Records a processed photo for a dish.</summary>
    /// <param name="dishId">The dish.</param>
    /// <param name="file">The stored image (already through the photo pipeline).</param>
    /// <param name="altText">Text for screen readers.</param>
    /// <param name="displayOrder">Position among the dish's photos (0 = first).</param>
    /// <exception cref="ArgumentOutOfRangeException">Alt text longer than <see cref="AltTextMaxLength"/>.</exception>
    internal DishPhoto(Guid dishId, ImageFile file, string altText, int displayOrder)
        : base(Guid.CreateVersion7())
    {
        DishId = dishId;
        File = file;
        AltText = CheckAltText(altText);
        DisplayOrder = displayOrder;
    }

    /// <summary>The dish this photo belongs to.</summary>
    public Guid DishId { get; private set; }

    /// <summary>Path, thumbnail, type, size, width, height and hash of the stored file.</summary>
    public ImageFile File { get; private set; } = null!;

    /// <summary>Read out by screen readers, e.g. "Jollof Rice &amp; Chicken from Mama Ama's Kitchen".</summary>
    public string AltText { get; private set; } = string.Empty;

    /// <summary>Lower numbers are shown first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>The photo shown on dish cards. Exactly one per dish.</summary>
    public bool IsPrimary { get; private set; }

    /// <summary>Changes the alt text (through Dish.UpdatePhotoAltText).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Longer than <see cref="AltTextMaxLength"/>.</exception>
    internal void UpdateAltText(string altText) => AltText = CheckAltText(altText);

    internal void SetDisplayOrder(int order) => DisplayOrder = order;
    internal void MarkPrimary() => IsPrimary = true;
    internal void ClearPrimary() => IsPrimary = false;

    /// <exception cref="ArgumentOutOfRangeException">Longer than <see cref="AltTextMaxLength"/> (the validator checks first).</exception>
    private static string CheckAltText(string altText)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(altText.Length, AltTextMaxLength, nameof(altText));
        return altText;
    }
}