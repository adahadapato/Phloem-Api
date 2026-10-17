using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Settings.Application;

namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>
/// Creates and checks signed, expiring links to PRIVATE files (vendor documents):
/// <c>{ApiBaseUrl}/private-files/{path}?expires={unix seconds}&amp;sig={HMAC-SHA256}</c>.
/// </summary>
/// <remarks>
/// <para>The signature covers the path AND the expiry time, so neither can be changed without breaking it.
/// Comparison is constant-time, so the signature can't be guessed byte by byte.</para>
/// <para>Public because Presentation's download endpoint calls <see cref="TryResolve"/> before streaming a file.
/// Any failure (bad path, expired, wrong signature, missing file) just returns false, and the endpoint answers 404
/// without saying why.</para>
/// </remarks>
public sealed class PrivateFileLinks(
    IOptions<StorageSettings> storage,
    IOptions<ApplicationSettings> application,
    LocalStoragePaths paths,
    TimeProvider time)
{
    /// <summary>A link to a private file that stops working after <paramref name="validFor"/>.</summary>
    /// <param name="path">The file's storage path.</param>
    /// <param name="validFor">How long the link works (e.g. 5 minutes).</param>
    /// <returns>The signed link.</returns>
    /// <exception cref="ArgumentException">An unsafe path.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A duration of zero or less.</exception>
    public Uri Create(string path, TimeSpan validFor)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(validFor, TimeSpan.Zero);

        var relative = LocalStoragePaths.Normalise(path);
        var expires = time.GetUtcNow().Add(validFor).ToUnixTimeSeconds();
        var signature = Sign(relative, expires);

        return new Uri($"{application.Value.ApiBaseUrl}{paths.PrivateRequestPath}/" +
                $"{LocalStoragePaths.ToUrlPath(relative)}?expires={expires}&sig={signature}");
    }

    /// <summary>Checks a link and, if it's valid, gives the file's full path on disk.</summary>
    /// <param name="path">The path part of the link (already URL-decoded by routing).</param>
    /// <param name="expires">The link's <c>expires</c> value.</param>
    /// <param name="signature">The link's <c>sig</c> value.</param>
    /// <param name="fullPath">The file on disk, when the link is valid.</param>
    /// <returns>True only if the path is safe, the link hasn't expired, the signature matches and the file exists.</returns>
    public bool TryResolve(string path, long expires, string? signature, [NotNullWhen(true)] out string? fullPath)
    {
        fullPath = null;
        if (string.IsNullOrWhiteSpace(signature)) return false;
        if (time.GetUtcNow().ToUnixTimeSeconds() > expires) return false;

        string relative;
        try { relative = LocalStoragePaths.Normalise(path); }
        catch (ArgumentException) { return false; }

        var expected = Encoding.ASCII.GetBytes(Sign(relative, expires));
        var given = Encoding.ASCII.GetBytes(signature);
        if (!CryptographicOperations.FixedTimeEquals(expected, given)) return false;

        var candidate = paths.ResolveFullPath(relative, StorageContainer.Private);
        if (!File.Exists(candidate)) return false;

        fullPath = candidate;
        return true;
    }

    /// <summary>HMAC-SHA256 of "path\nexpires" with the signing key, as URL-safe Base64.</summary>
    private string Sign(string relativePath, long expires)
    {
        var key = Encoding.UTF8.GetBytes(storage.Value.Local.SigningKey);
        var data = Encoding.UTF8.GetBytes($"{relativePath}\n{expires}");
        return Base64Url.EncodeToString(HMACSHA256.HashData(key, data));
    }
}