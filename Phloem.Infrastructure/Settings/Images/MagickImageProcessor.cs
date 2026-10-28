using System.Security.Cryptography;
using ImageMagick;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Infrastructure.Settings.Images;

/// <summary>
/// <see cref="IImageProcessor"/> with Magick.NET (ImageMagick, Apache 2.0). Accepts JPEG, PNG, WebP and HEIC;
/// always outputs WebP with no metadata (GPS, camera details).
/// </summary>
/// <remarks>
/// <para><b>Safety:</b> the upload is size-limited while it's read; its real format is detected from its bytes (never
/// the file name); only the four allowed formats are decoded, each with its own decoder only; and ImageMagick's
/// resource limits (set in <see cref="ImageSetup"/>) stop oversized "images" from eating the server.</para>
/// <para><b>Errors:</b> problems the user can fix return <see cref="ImageErrors"/>; nothing is thrown for bad uploads.</para>
/// </remarks>
internal sealed class MagickImageProcessor(IOptions<ImageSettings> options, ILogger<MagickImageProcessor> logger)
    : IImageProcessor
{
    /// <summary>What we store: always WebP.</summary>
    private const string OutputContentType = "image/webp";

    /// <summary>The only formats we agree to decode.</summary>
    private static readonly HashSet<MagickFormat> AllowedFormats =
    [
        MagickFormat.Jpeg, MagickFormat.Jpg, MagickFormat.Pjpeg,
        MagickFormat.Png, MagickFormat.Png8, MagickFormat.Png24, MagickFormat.Png32,
        MagickFormat.WebP,
        MagickFormat.Heic, MagickFormat.Heif
    ];

    /// <inheritdoc />
    /// <remarks>
    /// Read (≤ limit) → check format → decode → upright → white behind transparency → crop 4:3 (centre) →
    /// reject if the crop is below <see cref="DishPhotoStandard.MinimumUpload"/> → reject if blurry →
    /// <see cref="DishPhotoStandard.FullSize"/> + <see cref="DishPhotoStandard.Thumbnail"/> → light sharpen →
    /// WebP, no metadata → SHA-256 of the full-size file.
    /// </remarks>
    public async Task<Result<ProcessedImage>> ProcessDishPhotoAsync(Stream upload, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        var bytes = await ReadLimitedAsync(upload, settings.MaxDishPhotoBytes, cancellationToken);
        if (bytes is null) return ImageErrors.TooLarge(settings.MaxDishPhotoBytes);

        var decoded = Decode(bytes);
        if (decoded.IsFailure) return decoded.Error;

        using var original = decoded.Value;
        using var image = OnWhite(original);  // no see-through corners on food photos

        // Crop to 4:3 first, then check the size of what's left
        var upright = ImageDimensions.Create((int)image.Width, (int)image.Height);
        var crop = upright.CropToAspect(DishPhotoStandard.RatioWidth, DishPhotoStandard.RatioHeight);
        if (!crop.IsAtLeast(DishPhotoStandard.MinimumUpload.Width, DishPhotoStandard.MinimumUpload.Height))
            return ImageErrors.TooSmall;

        image.Crop(new MagickGeometry((uint)crop.Width, (uint)crop.Height), Gravity.Center);
        image.ResetPage();

        var sharpness = BlurDetector.Sharpness(image);
        if (sharpness < settings.BlurThreshold)
        {
            logger.LogInformation("Dish photo rejected as blurry (sharpness {Sharpness:0.0} < {Threshold})",
                                  sharpness, settings.BlurThreshold);
            return ImageErrors.TooBlurry;
        }

        using var thumbnail = image.Clone();
        ResizeExactly(image, DishPhotoStandard.FullSize);
        ResizeExactly(thumbnail, DishPhotoStandard.Thumbnail);

        if (settings.EnhanceDishPhotos)
        {
            image.UnsharpMask(0, 0.75, 0.75, 0.008);     // subtle: crisper food, no halos
            thumbnail.UnsharpMask(0, 0.5, 0.6, 0.008);
        }

        var full = Encode(image, settings.WebpQuality);
        var thumb = Encode(thumbnail, settings.WebpQuality);

        return new ProcessedImage(full, thumb, OutputContentType, DishPhotoStandard.FullSize, full.Length, Hash(full));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Read (≤ limit) → check format → decode → upright → shrink so the longest side ≤
    /// <see cref="ImageSettings.SimpleImageMaxSide"/> (never enlarged) → WebP, no metadata, transparency KEPT
    /// (logos on transparent backgrounds stay that way) → SHA-256.
    /// </remarks>
    public async Task<Result<ProcessedImage>> ProcessSimpleImageAsync(Stream upload, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        var bytes = await ReadLimitedAsync(upload, settings.MaxSimpleImageBytes, cancellationToken);
        if (bytes is null) return ImageErrors.TooLarge(settings.MaxSimpleImageBytes);

        var decoded = Decode(bytes);
        if (decoded.IsFailure) return decoded.Error;

        using var image = decoded.Value;
        var maxSide = (uint)settings.SimpleImageMaxSide;
        if (image.Width > maxSide || image.Height > maxSide)
            image.Resize(new MagickGeometry(maxSide, maxSide));                // fits inside, keeps the shape

        var full = Encode(image, settings.WebpQuality);
        var dimensions = ImageDimensions.Create((int)image.Width, (int)image.Height);

        return new ProcessedImage(full, null, OutputContentType, dimensions, full.Length, Hash(full));
    }

    /// <inheritdoc />
    /// <remarks>
    /// Read (≤ limit) → check format → decode (upright, metadata removed) → white behind transparency → shrink so the
    /// longest side ≤ <see cref="ImageSettings.DeliveryPhotoMaxSide"/> (never enlarged) → WebP → SHA-256.
    /// </remarks>
    public async Task<Result<ProcessedImage>> ProcessDeliveryPhotoAsync(Stream upload,
                                                                       CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        var bytes = await ReadLimitedAsync(upload, settings.MaxDeliveryPhotoBytes, cancellationToken);
        if (bytes is null) return ImageErrors.TooLarge(settings.MaxDeliveryPhotoBytes);

        var decoded = Decode(bytes);
        if (decoded.IsFailure) return decoded.Error;

        using var original = decoded.Value;
        using var image = OnWhite(original);
        var maxSide = (uint)settings.DeliveryPhotoMaxSide;
        if (image.Width > maxSide || image.Height > maxSide)
            image.Resize(new MagickGeometry(maxSide, maxSide));                // fits inside, keeps the shape

        var full = Encode(image, settings.WebpQuality);
        var dimensions = ImageDimensions.Create((int)image.Width, (int)image.Height);

        return new ProcessedImage(full, null, OutputContentType, dimensions, full.Length, Hash(full));
    }

    // =====================================================================
    // Steps
    // =====================================================================

    /// <summary>Reads the upload into memory, stopping as soon as it passes the limit.</summary>
    /// <param name="upload">The uploaded stream (not disposed here: the caller owns it).</param>
    /// <param name="maxBytes">The limit.</param>
    /// <param name="cancellationToken">Cancels reading.</param>
    /// <returns>The bytes; or null if the upload is over the limit.</returns>
    private static async Task<byte[]?> ReadLimitedAsync(Stream upload, long maxBytes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(upload);
        if (upload.CanSeek && upload.Length - upload.Position > maxBytes) return null;

        using var buffer = new MemoryStream();
        var chunk = new byte[81_920];
        int read;
        while ((read = await upload.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > maxBytes) return null;
            buffer.Write(chunk, 0, read);
        }
        return buffer.ToArray();
    }

    /// <summary>
    /// Detects the real format from the bytes, refuses anything not allowed, decodes with that format's decoder
    /// only, turns the photo upright (phone rotation) and removes all metadata.
    /// </summary>
    /// <param name="bytes">The upload.</param>
    /// <returns>The decoded image (caller disposes it); or <see cref="ImageErrors.NotAnImage"/>.</returns>
    private Result<MagickImage> Decode(byte[] bytes)
    {
        if (bytes.Length == 0) return ImageErrors.NotAnImage;

        try
        {
            var info = new MagickImageInfo(bytes);
            if (!AllowedFormats.Contains(info.Format)) return ImageErrors.NotAnImage;

            var image = new MagickImage(bytes, new MagickReadSettings { Format = info.Format });
            image.AutoOrient();     // apply the phone's rotation flag to the pixels
            image.Strip();          // drop EXIF/GPS/ICC metadata
            return image;
        }
        catch (MagickException ex)
        {
            // Unreadable, corrupt, or over the resource limits: all mean "pick another photo"
            logger.LogInformation(ex, "Upload rejected: not a readable image");
            return ImageErrors.NotAnImage;
        }
    }

    /// <summary>Resizes to exactly the given size (the shape already matches after the 4:3 crop).</summary>
    private static void ResizeExactly(IMagickImage<byte> image, ImageDimensions size) =>
        image.Resize(new MagickGeometry((uint)size.Width, (uint)size.Height) { IgnoreAspectRatio = true });

    /// <summary>
    /// A copy of the photo drawn onto a white canvas of the same size, so any transparent areas become white
    /// (food photos must never have see-through corners). The caller disposes the result.
    /// </summary>
    /// <param name="source">The decoded, upright photo (left unchanged).</param>
    /// <returns>An opaque copy.</returns>
    private static MagickImage OnWhite(IMagickImage<byte> source)
    {
        var canvas = new MagickImage(MagickColors.White, source.Width, source.Height);
        canvas.Composite(source, CompositeOperator.Over);
        return canvas;
    }

    /// <summary>Writes the image as WebP into a new stream positioned at the start.</summary>
    private static MemoryStream Encode(IMagickImage<byte> image, int quality)
    {
        image.Quality = (uint)quality;
        var output = new MemoryStream();
        image.Write(output, MagickFormat.WebP);
        output.Position = 0;
        return output;
    }

    /// <summary>SHA-256 of the stream's contents as upper-case hex (64 characters); leaves the stream at the start.</summary>
    private static string Hash(MemoryStream stream)
    {
        var hash = Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, (int)stream.Length)));
        stream.Position = 0;
        return hash;
    }
}