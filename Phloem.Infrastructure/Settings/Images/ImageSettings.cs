using Phloem.Application.Common;
using Phloem.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace Phloem.Infrastructure.Settings.Images;

/// <summary>
/// The <c>"Images"</c> section (keys in <see cref="AppSettings.Images"/>): tunable numbers for the photo pipeline.
/// </summary>
/// <remarks>
/// Output SIZES are not here: they're business rules in the Domain (<see cref="DishPhotoStandard"/>), which
/// Dish.AddPhoto enforces. Only limits and quality knobs live in settings.
/// </remarks>
public sealed class ImageSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Images.Section;

    /// <summary>Largest dish-photo upload in bytes (default 10 MB).</summary>
    [Range(1, 50 * 1024 * 1024)]
    public long MaxDishPhotoBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Largest profile-photo / logo upload in bytes (default 5 MB).</summary>
    [Range(1, 50 * 1024 * 1024)]
    public long MaxSimpleImageBytes { get; set; } = 5 * 1024 * 1024;

    /// <summary>Profile photos and logos are shrunk so their longest side is at most this (default 800 px).</summary>
    [Range(100, 4000)]
    public int SimpleImageMaxSide { get; set; } = 800;

    /// <summary>Largest proof-of-delivery photo upload in bytes (default 10 MB: phone cameras take big photos).</summary>
    [Range(1, 50 * 1024 * 1024)]
    public long MaxDeliveryPhotoBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>Proof-of-delivery photos are shrunk so their longest side is at most this (default 1600 px).</summary>
    [Range(400, 4000)]
    public int DeliveryPhotoMaxSide { get; set; } = 1600;

    /// <summary>WebP quality 1–100 (default 82: visually clean, small files).</summary>
    [Range(1, 100)]
    public int WebpQuality { get; set; } = 82;

    /// <summary>
    /// Sharpness needed to accept a dish photo (variance of the Laplacian; higher = sharper).
    /// Lower it if good photos get rejected, raise it if blurry ones get through. Default 60.
    /// </summary>
    [Range(0, 10_000)]
    public double BlurThreshold { get; set; } = 60;

    /// <summary>Apply a light sharpen to dish photos after resizing (default true).</summary>
    public bool EnhanceDishPhotos { get; set; } = true;
}