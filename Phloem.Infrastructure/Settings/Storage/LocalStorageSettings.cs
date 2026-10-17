using System.ComponentModel.DataAnnotations;
using Phloem.Application.Common.Settings;

namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>
/// The <c>"Storage:Local"</c> sub-section (keys in <see cref="AppSettings.Storage.Local"/>): settings for
/// <see cref="StorageProvider.Local"/>. Every folder and web path is set here, never in code.
/// </summary>
/// <remarks>
/// Folder paths are relative to the API's content root (the Presentation project folder).
/// Both folders must stay out of source control (see .gitignore).
/// </remarks>
public sealed class LocalStorageSettings
{
    /// <summary>Shortest signing key accepted (characters).</summary>
    public const int MinSigningKeyLength = 32;

    /// <summary>
    /// Folder for PUBLIC files (dish photos, logos, profile photos). Must be inside the web root (wwwroot) so
    /// ASP.NET Core serves them as static files; its place in the web root becomes the web path
    /// ("wwwroot/uploads" → {ApiBaseUrl}/uploads/...).
    /// </summary>
    [Required]
    public string PublicRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Folder for PRIVATE files (vendor documents). On shared hosting it has to live inside the web root (the only
    /// writable folder), so Presentation BLOCKS direct requests to it; files are only reachable through signed,
    /// expiring links. If your host allows a folder outside the web root, use that instead: it's safer still.
    /// </summary>
    [Required]
    public string PrivateRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Web path of the signed download endpoint, e.g. "/private-files" → {ApiBaseUrl}/private-files/...?expires=…&amp;sig=….
    /// Starts with "/", no trailing slash.
    /// </summary>
    [Required, RegularExpression("^/[A-Za-z0-9_-]+(/[A-Za-z0-9_-]+)*$",
        ErrorMessage = "Storage:Local:PrivateDownloadPath must look like \"/private-files\" (leading slash, no trailing slash).")]
    public string PrivateDownloadPath { get; set; } = string.Empty;

    /// <summary>
    /// Secret used to sign private-file links (HMAC-SHA256), at least <see cref="MinSigningKeyLength"/> characters.
    /// SECRET: User Secrets / environment variables only. Changing it makes every link already given out stop working.
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;
}