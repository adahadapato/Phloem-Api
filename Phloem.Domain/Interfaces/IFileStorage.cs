namespace Phloem.Domain.Interfaces;

/// <summary>Which container a file lives in.</summary>
public enum StorageContainer
{
    /// <summary>Dish photos, shop logo / cover, profile photos: anyone can view.</summary>
    Public = 1,

    /// <summary>Vendor documents: only through short-lived signed links.</summary>
    Private = 2
}

/// <summary>
/// Saves and deletes files. Implemented in Infrastructure (local folder in development,
/// Azure Blob Storage in production). The database only ever stores the returned path.
/// </summary>
public interface IFileStorage
{
    /// <summary>Saves the stream at the given path (e.g. "dishes/{dishId}/{photoId}.webp"); returns the path.</summary>
    Task<string> SaveAsync(Stream content, string path, string contentType, StorageContainer container,
                           CancellationToken cancellationToken = default);

    /// <summary>Deletes a file. Deleting a file that's already gone is not an error.</summary>
    Task DeleteAsync(string path, StorageContainer container, CancellationToken cancellationToken = default);

    /// <summary>Permanent URL for a public file (what the app loads dish photos from).</summary>
    string GetPublicUrl(string path);

    /// <summary>A link to a private file that stops working after validFor (e.g. 5 minutes).</summary>
    Task<Uri> GetTemporaryUrlAsync(string path, TimeSpan validFor, CancellationToken cancellationToken = default);
}