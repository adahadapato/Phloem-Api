using Phloem.Domain.Constants;

namespace Phloem.Domain.Shared.Errors;

/// <summary>Photo problems the user can fix themselves, returned by IImageProcessor.</summary>
public static class ImageErrors
{
    public static readonly Error NotAnImage = Error.Validation(
        "Image.NotAnImage", "That file isn't a photo we can read. Please use a JPEG, PNG or WebP image.");

    public static readonly Error TooSmall = Error.Validation(
        "Image.TooSmall",
        $"This photo is too small. Please use one at least {DishPhotoStandard.MinimumUpload.Width}×{DishPhotoStandard.MinimumUpload.Height} pixels.");

    public static readonly Error TooBlurry = Error.Validation(
        "Image.TooBlurry", "This photo is too blurry. Try again in good light and hold the phone steady.");
}