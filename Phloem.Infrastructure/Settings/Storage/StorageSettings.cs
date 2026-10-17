using Phloem.Application.Common;

namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>
/// The <c>"Storage"</c> section (keys in <see cref="AppSettings.Storage"/>): which provider keeps uploaded
/// files, plus that provider's own settings.
/// </summary>
public sealed class StorageSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Storage.Section;

    /// <summary>Which provider to use. Default: <see cref="StorageProvider.Local"/>.</summary>
    public StorageProvider Provider { get; set; } = StorageProvider.Local;

    /// <summary>Settings used when <see cref="Provider"/> is <see cref="StorageProvider.Local"/>.</summary>
    public LocalStorageSettings Local { get; set; } = new();
}