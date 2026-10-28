using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// The image pipeline (Infrastructure, Magick.NET). Does NOT store anything: it returns processed bytes, and the
/// use case saves them with IFileStorage. Accepts JPEG, PNG, WebP and HEIC (iPhone); always outputs WebP.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Dish photos: reject small / blurry, fix rotation, crop 4:3, resize to 1200×900 + 400×300 thumbnail,
    /// light enhance, WebP, strip metadata. Fails with ImageErrors for things the seller can fix.
    /// </summary>
    Task<Result<ProcessedImage>> ProcessDishPhotoAsync(Stream upload, CancellationToken cancellationToken = default);

    /// <summary>Profile photos and shop logos: no enhancement; strip GPS, shrink to max ~800 px.</summary>
    Task<Result<ProcessedImage>> ProcessSimpleImageAsync(Stream upload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Proof-of-delivery photos (a front door, a parcel on a step): upright, shrink to max ~1600 px so details stay
    /// readable, WebP, ALL metadata removed (the phone's GPS would give away the exact spot). No blur check: a
    /// slightly soft photo at night is still proof.
    /// </summary>
    Task<Result<ProcessedImage>> ProcessDeliveryPhotoAsync(Stream upload, CancellationToken cancellationToken = default);
}

/// <summary>What the pipeline produces. Streams are owned by the caller: dispose after saving.</summary>
public sealed record ProcessedImage(
    Stream FullSize,
    Stream? Thumbnail,
    string ContentType,          // "image/webp"
    ImageDimensions Dimensions,
    long FileSizeBytes,
    string Hash) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await FullSize.DisposeAsync();
        if (Thumbnail is not null) await Thumbnail.DisposeAsync();
    }
}