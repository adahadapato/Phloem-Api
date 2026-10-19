
using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// Configuration for ASP.NET Core Data Protection.
/// Determines where encryption keys are stored.
/// </summary>
public sealed class DataProtectionSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName =
        AppSettings.DataProtection.Section;

    /// <summary>
    /// Application name used to isolate Phloem's
    /// Data Protection keys from other applications.
    /// </summary>
    [Required]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Directory used to store Data Protection keys.
    /// </summary>
    [Required]
    public string KeysPath { get; set; } = string.Empty;

    /// <summary>
    /// Resolves the key directory and creates it
    /// automatically if it does not already exist.
    /// </summary>
    public string ResolveKeysPath()
    {
        if (string.IsNullOrWhiteSpace(KeysPath))
        {
            throw new InvalidOperationException(
                "DataProtection:KeysPath must be configured.");
        }

        var fullPath = Path.IsPathRooted(KeysPath)
            ? Path.GetFullPath(KeysPath)
            : Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, KeysPath));

        // Safe to call even if the directory already exists.
        Directory.CreateDirectory(fullPath);

        return fullPath;
    }
}
