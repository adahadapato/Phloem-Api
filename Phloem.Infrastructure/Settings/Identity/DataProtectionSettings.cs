using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// The <c>"DataProtection"</c> section of appsettings.json: where the keys that sign email / reset tokens and the
/// 2FA challenge are kept. Keys are listed in <see cref="AppSettings.DataProtection"/>.
/// </summary>
/// <remarks>
/// <para>Without a folder, shared hosting keeps the keys in memory only: every restart (deploys, idle shutdowns)
/// would make links already emailed ("confirm your email", "reset your password") stop working.</para>
/// <para>The folder is created on first start. Keep it OUT of <c>wwwroot</c> and never delete it on deploy
/// (the deploy workflow keeps files it didn't publish).</para>
/// </remarks>
public sealed class DataProtectionSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.DataProtection.Section;

    /// <summary>Isolates Phloem's keys from other apps, e.g. "Phloem". Changing it makes old tokens invalid.</summary>
    [Required]
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>Folder for the key files: relative to the app folder (e.g. "App_Data/keys") or a full path.</summary>
    [Required]
    public string KeysPath { get; set; } = string.Empty;

    /// <summary>The full folder path: <see cref="KeysPath"/> as is if it's a full path, otherwise next to the app.</summary>
    /// <returns>An absolute folder path.</returns>
    public string ResolveKeysPath() =>
        Path.IsPathRooted(KeysPath) ? KeysPath : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, KeysPath));
}
