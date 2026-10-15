using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// One photo of a dish. The image file lives in storage; this row records where it is,
/// its details, and how it is shown (alt text, order, which one is the main photo).
/// Belongs to a Dish and is only created / changed through it.
/// </summary>
public class DishPhoto : Entity<Guid>
{
    protected DishPhoto() { }

    internal DishPhoto(Guid dishId, ImageFile file, string altText, int displayOrder)
        : base(Guid.CreateVersion7())
    {
        DishId = dishId;
        File = file;
        AltText = altText;
        DisplayOrder = displayOrder;
    }

    public Guid DishId { get; private set; }

    /// <summary>Path, thumbnail, type, size, width, height and hash of the stored file.</summary>
    public ImageFile File { get; private set; } = null!;

    /// <summary>Read out by screen readers, e.g. "Jollof Rice & Chicken from Mama Ama's Kitchen".</summary>
    public string AltText { get; private set; } = string.Empty;

    /// <summary>Lower numbers are shown first.</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>The photo shown on dish cards. Exactly one per dish.</summary>
    public bool IsPrimary { get; private set; }

    internal void UpdateAltText(string altText) => AltText = altText;
    internal void SetDisplayOrder(int order) => DisplayOrder = order;
    internal void MarkPrimary() => IsPrimary = true;
    internal void ClearPrimary() => IsPrimary = false;
}