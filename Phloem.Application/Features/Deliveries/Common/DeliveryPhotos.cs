using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Deliveries.Common;

/// <summary>
/// Proof-of-delivery photos: processes and saves them in PRIVATE storage, removes them again if the delivery is
/// refused, and turns a stored proof into what the app shows (with a short-lived photo link).
/// </summary>
/// <remarks>Private, because the photo shows someone's home: nobody can open it without a fresh link from the API,
/// and links stop working after <see cref="LinkLifetime"/>.</remarks>
public sealed class DeliveryPhotos(IImageProcessor images, IFileStorage storage, TimeProvider time)
{
    /// <summary>How long a photo link works (long enough to look at the page, short enough not to be shared on).</summary>
    public static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(15);

    /// <summary>Processes the upload (upright, GPS removed, ≤ 1600 px, WebP) and saves it.</summary>
    /// <param name="upload">The photo from the courier's camera.</param>
    /// <param name="folder">"orders" or "parcels".</param>
    /// <param name="reference">The order or tracking number (kept in the file name to make support easier).</param>
    /// <param name="ct">Cancels processing and saving.</param>
    /// <returns>The storage path; or an <c>ImageErrors</c> error the courier can fix (not a photo, too large).</returns>
    public async Task<Result<string>> SaveAsync(Stream upload, string folder, string reference, CancellationToken ct)
    {
        var processed = await images.ProcessDeliveryPhotoAsync(upload, ct);
        if (processed.IsFailure) return processed.Error;

        await using var image = processed.Value;
        var now = time.GetUtcNow().UtcDateTime;
        var safeReference = new string(reference.Where(char.IsLetterOrDigit).ToArray());
        var path = $"deliveries/{folder}/{now:yyyy}/{now:MM}/{safeReference}-{Guid.CreateVersion7():N}.webp";

        await storage.SaveAsync(image.FullSize, path, image.ContentType, StorageContainer.Private, ct);
        return path;
    }

    /// <summary>Removes a photo that was saved for a delivery the Domain then refused.</summary>
    /// <param name="path">The path from <see cref="SaveAsync"/>.</param>
    /// <param name="ct">Cancels the call.</param>
    public Task DeleteAsync(string path, CancellationToken ct) =>
        storage.DeleteAsync(path, StorageContainer.Private, ct);

    /// <summary>What the order / parcel page shows; null when there's no proof yet.</summary>
    /// <param name="proof">The stored proof.</param>
    /// <param name="courierName">Who delivered it.</param>
    /// <param name="ct">Cancels the call.</param>
    public async Task<DeliveryProofDto?> ToDtoAsync(ProofOfDelivery? proof, string? courierName, CancellationToken ct)
    {
        if (proof is null) return null;

        var link = await storage.GetTemporaryUrlAsync(proof.PhotoPath, LinkLifetime, ct);
        return new DeliveryProofDto(link.ToString(), proof.Handover, proof.Describe(), proof.ReceivedBy, proof.Note,
                                    courierName, proof.CapturedAtUtc);
    }
}
