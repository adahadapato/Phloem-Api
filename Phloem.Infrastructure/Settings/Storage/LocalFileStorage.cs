using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;
using Phloem.Infrastructure.Settings.Application;

namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>
/// <see cref="IFileStorage"/> on the API's own disk. Public files are served as static files at
/// <see cref="LocalStoragePaths.PublicRequestPath"/>; private files only through <see cref="PrivateFileLinks"/>.
/// </summary>
/// <remarks>
/// <para>The database stores the storage PATH (e.g. "dishes/{dishId}/{photoId}.webp"), never a full URL, so moving
/// to a cloud provider later only changes how URLs are built.</para>
/// <para>Every path goes through <see cref="LocalStoragePaths"/>, so nothing can be written or deleted outside the
/// storage folders.</para>
/// </remarks>
internal sealed class LocalFileStorage(
    LocalStoragePaths paths,
    PrivateFileLinks links,
    IOptions<ApplicationSettings> application,
    ILogger<LocalFileStorage> logger) : IFileStorage
{
    /// <summary>Copy buffer size (80 KB, .NET's default for stream copies).</summary>
    private const int BufferSize = 81_920;

    /// <inheritdoc />
    /// <remarks>Overwrites an existing file at the same path. A failed or cancelled save leaves no half-written file.</remarks>
    /// <exception cref="ArgumentNullException">No content.</exception>
    /// <exception cref="ArgumentException">An unsafe path or blank content type.</exception>
    public async Task<string> SaveAsync(Stream content, string path, string contentType, StorageContainer container,
                                        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);   // the file extension carries the type on disk

        var relative = LocalStoragePaths.Normalise(path);
        var fullPath = paths.ResolveFullPath(relative, container);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        if (content.CanSeek) content.Position = 0;

        try
        {
            await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None,
                                                  BufferSize, useAsync: true);
            await content.CopyToAsync(file, BufferSize, cancellationToken);
        }
        catch
        {
            TryDelete(fullPath);
            throw;
        }

        logger.LogInformation("Stored file {Path} ({Container})", relative, container);
        return relative;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">An unsafe path.</exception>
    public Task DeleteAsync(string path, StorageContainer container, CancellationToken cancellationToken = default)
    {
        var fullPath = paths.ResolveFullPath(path, container);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            logger.LogInformation("Deleted file {Path} ({Container})", path, container);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">An unsafe path.</exception>
    public string GetPublicUrl(string path)
    {
        var relative = LocalStoragePaths.Normalise(path);
        return $"{application.Value.ApiBaseUrl}{paths.PublicRequestPath}/{LocalStoragePaths.ToUrlPath(relative)}";
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">An unsafe path.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A duration of zero or less.</exception>
    public Task<Uri> GetTemporaryUrlAsync(string path, TimeSpan validFor, CancellationToken cancellationToken = default) =>
        Task.FromResult(links.Create(path, validFor));

    /// <summary>Removes a half-written file after a failed save; never throws.</summary>
    /// <param name="fullPath">The file on disk.</param>
    private void TryDelete(string fullPath)
    {
        try { if (File.Exists(fullPath)) File.Delete(fullPath); }
        catch (Exception ex) { logger.LogWarning(ex, "Couldn't remove half-written file {FullPath}", fullPath); }
    }
}