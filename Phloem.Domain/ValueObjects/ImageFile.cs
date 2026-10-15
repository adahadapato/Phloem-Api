namespace Phloem.Domain.ValueObjects;

/// <summary>
/// Facts about an image file that has already been saved to storage
/// (the file itself is NOT in the database, only where it is and what it is).
/// Validated once here, so every image owner (dish photos, shop logo / cover) shares the checks.
/// </summary>
public sealed class ImageFile : ValueObject
{
    /// <summary>Formats the app accepts (phones produce JPEG; dish photos are converted to WebP).</summary>
    public static readonly IReadOnlyList<string> AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp"];

    public const string InvalidFormatMessage = "Please upload a JPEG, PNG or WebP photo.";

    private ImageFile(string path, string? thumbnailPath, string contentType,
                      long fileSizeBytes, ImageDimensions dimensions, string hash)
    {
        Path = path;
        ThumbnailPath = thumbnailPath;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        Dimensions = dimensions;
        Hash = hash;
    }

    /// <summary>Where the full-size file lives, e.g. "dishes/{dishId}/{photoId}.webp".</summary>
    public string Path { get; }

    /// <summary>Small version for lists and order history.</summary>
    public string? ThumbnailPath { get; }

    public string ContentType { get; }
    public long FileSizeBytes { get; }

    /// <summary>Width × height in pixels.</summary>
    public ImageDimensions Dimensions { get; }

    /// <summary>SHA-256 of the file (worked out by Infrastructure). Used to spot the same photo uploaded twice.</summary>
    public string Hash { get; }

    public static ImageFile Create(string path, string? thumbnailPath, string contentType,
                                   long fileSizeBytes, ImageDimensions dimensions, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        ArgumentNullException.ThrowIfNull(dimensions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeBytes);

        var type = contentType.Trim().ToLowerInvariant();
        if (!AllowedContentTypes.Contains(type))
            throw new ArgumentException(InvalidFormatMessage, nameof(contentType));

        return new ImageFile(path.Trim(),
            string.IsNullOrWhiteSpace(thumbnailPath) ? null : thumbnailPath.Trim(),
            type, fileSizeBytes, dimensions, hash.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Path;
        yield return ThumbnailPath;
        yield return ContentType;
        yield return FileSizeBytes;
        yield return Dimensions;
        yield return Hash;
    }
}