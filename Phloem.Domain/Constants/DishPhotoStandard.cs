using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Constants;

/// <summary>
/// The one size every dish photo is turned into by the image pipeline (Infrastructure),
/// so all dish cards line up and load fast. Dish.AddPhoto refuses anything else,
/// which guarantees no unprocessed photo is ever attached to a dish.
/// </summary>
public static class DishPhotoStandard
{
    /// <summary>Shape of every dish photo: 4:3.</summary>
    public const int RatioWidth = 4;
    public const int RatioHeight = 3;

    /// <summary>Full-size photo for the dish details page.</summary>
    public static readonly ImageDimensions FullSize = ImageDimensions.Create(1200, 900);

    /// <summary>Thumbnail for listings, shop pages and order history.</summary>
    public static readonly ImageDimensions Thumbnail = ImageDimensions.Create(400, 300);

    /// <summary>Uploads smaller than this are rejected before processing (they'd look blurry).</summary>
    public static readonly ImageDimensions MinimumUpload = ImageDimensions.Create(800, 600);
}