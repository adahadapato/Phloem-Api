namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>Where uploaded files are kept. Chosen with the <c>Storage:Provider</c> setting.</summary>
/// <remarks>Add a value here (and its implementation in <see cref="StorageSetup"/>) when a cloud provider is chosen.</remarks>
public enum StorageProvider
{
    /// <summary>A folder on the API's own disk (development, or a single-server host).</summary>
    Local = 1
}