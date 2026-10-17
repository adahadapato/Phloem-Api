using Phloem.Domain.Constants;

namespace Phloem.Domain.Shared.Errors;

/// <summary>Photo problems the user can fix themselves, returned by IImageProcessor.</summary>
/// <remarks>All are Validation errors (HTTP 400): the app shows the message and lets the user pick another photo.</remarks>
public static class ImageErrors
{
    /// <summary>The file isn't a readable JPEG, PNG, WebP or HEIC image.</summary>
    public static readonly Error NotAnImage = Error.Validation(
        "Image.NotAnImage", "That file isn't a photo we can read. Please use a JPEG, PNG, WebP or HEIC image.");

    /// <summary>The photo is smaller than <see cref="DishPhotoStandard.MinimumUpload"/> (after cropping to 4:3).</summary>
    public static readonly Error TooSmall = Error.Validation(
        "Image.TooSmall",
        $"This photo is too small. Please use one at least {DishPhotoStandard.MinimumUpload.Width}×{DishPhotoStandard.MinimumUpload.Height} pixels.");

    /// <summary>The photo fails the sharpness test.</summary>
    public static readonly Error TooBlurry = Error.Validation(
        "Image.TooBlurry", "This photo is too blurry. Try again in good light and hold the phone steady.");

    /// <summary>The file is bigger than the upload limit.</summary>
    /// <param name="maxBytes">The limit in bytes.</param>
    /// <returns>The error, with the limit in MB.</returns>
    public static Error TooLarge(long maxBytes) => Error.Validation(
        "Image.TooLarge", $"This photo is too big. The maximum is {maxBytes / (1024 * 1024)} MB.");
}