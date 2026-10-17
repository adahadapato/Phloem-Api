using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;

namespace Phloem.Infrastructure.Settings.Storage;

/// <summary>
/// Where local files live on disk and under which web paths, plus SAFE path handling: every path is checked so it
/// can never reach outside its storage folder (no "..", no absolute paths).
/// </summary>
/// <remarks>
/// <para>Nothing here is hard-coded: folders and the download path come from <see cref="LocalStorageSettings"/>,
/// and the web root comes from ASP.NET Core (<see cref="IWebHostEnvironment.WebRootPath"/>).</para>
/// <para>Public files sit inside the web root, so Presentation's <c>UseStaticFiles()</c> serves them at
/// <see cref="PublicRequestPath"/>. Private files are only served by Presentation's download endpoint at
/// <see cref="PrivateRequestPath"/>; if their folder is inside the web root, Presentation must block
/// <see cref="BlockedRequestPath"/> before <c>UseStaticFiles()</c>.</para>
/// </remarks>
public sealed class LocalStoragePaths
{
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    /// <summary>Works out both folders and web paths from the settings, checks them, and creates the folders.</summary>
    /// <param name="options">Storage settings.</param>
    /// <param name="environment">ASP.NET Core's environment: content root and web root.</param>
    /// <exception cref="InvalidOperationException">
    /// No web root folder, the public folder isn't inside it, or the two folders overlap.
    /// </exception>
    public LocalStoragePaths(IOptions<StorageSettings> options, IWebHostEnvironment environment)
    {
        var local = options.Value.Local;

        var webRoot = environment.WebRootPath
            ?? throw new InvalidOperationException(
                "The API has no web root folder. Create 'wwwroot' in Phloem.Presentation (it can't be added while running).");
        webRoot = Path.GetFullPath(webRoot);

        PublicRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, local.PublicRootPath));
        PrivateRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, local.PrivateRootPath));

        if (!IsInside(PublicRoot, webRoot))
            throw new InvalidOperationException($"Storage:Local:PublicRootPath must be a folder inside the web root ({webRoot}).");
        if (IsInside(PrivateRoot, PublicRoot) || IsInside(PublicRoot, PrivateRoot) || PrivateRoot.Equals(PublicRoot, PathComparison))
            throw new InvalidOperationException("Storage:Local public and private folders must be separate (neither inside the other).");

        PublicRequestPath = ToRequestPath(webRoot, PublicRoot);
        PrivateRequestPath = local.PrivateDownloadPath;
        BlockedRequestPath = IsInside(PrivateRoot, webRoot) ? ToRequestPath(webRoot, PrivateRoot) : null;

        Directory.CreateDirectory(PublicRoot);
        Directory.CreateDirectory(PrivateRoot);
    }

    /// <summary>Folder for public files (dish photos, logos, profile photos).</summary>
    public string PublicRoot { get; }

    /// <summary>Folder for private files (vendor documents). Never served directly.</summary>
    public string PrivateRoot { get; }

    /// <summary>Web path of the public folder, worked out from its place in the web root (e.g. "/uploads").</summary>
    public string PublicRequestPath { get; }

    /// <summary>Web path of the signed download endpoint, from settings (e.g. "/private-files").</summary>
    public string PrivateRequestPath { get; }

    /// <summary>
    /// When <see cref="PrivateRoot"/> is inside the web root, the web path Presentation must block (e.g.
    /// "/private-uploads"); null when it's outside and nothing needs blocking.
    /// </summary>
    public string? BlockedRequestPath { get; }

    /// <summary>
    /// Cleans a storage path ("dishes/abc/1.webp"): forward slashes, no leading slash, and refuses anything unsafe.
    /// </summary>
    /// <param name="path">The path as given by Application.</param>
    /// <returns>The clean relative path.</returns>
    /// <exception cref="ArgumentException">Blank, absolute, or containing "", ".", ".." or ":" segments.</exception>
    public static string Normalise(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var clean = path.Trim().Replace('\\', '/').TrimStart('/');
        var segments = clean.Split('/');

        if (clean.Length == 0 || Path.IsPathRooted(clean) ||
            segments.Any(s => s.Length == 0 || s == "." || s == ".." || s.Contains(':')))
            throw new ArgumentException($"'{path}' isn't a valid storage path.", nameof(path));

        return clean;
    }

    /// <summary>The full disk path for a storage path, guaranteed to be inside the container's folder.</summary>
    /// <param name="path">The storage path.</param>
    /// <param name="container">Public or private.</param>
    /// <returns>The full path on disk.</returns>
    /// <exception cref="ArgumentException">An unsafe path.</exception>
    public string ResolveFullPath(string path, StorageContainer container)
    {
        var relative = Normalise(path);
        var root = RootFor(container);
        var full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));

        // Belt and braces: even after Normalise, never allow a path outside the root
        if (!IsInside(full, root))
            throw new ArgumentException($"'{path}' points outside the storage folder.", nameof(path));

        return full;
    }

    /// <summary>A storage path made safe for a URL (each segment escaped, slashes kept).</summary>
    /// <param name="relativePath">A path already passed through <see cref="Normalise"/>.</param>
    /// <returns>The URL-encoded path.</returns>
    public static string ToUrlPath(string relativePath) =>
        string.Join('/', relativePath.Split('/').Select(Uri.EscapeDataString));

    /// <summary>The folder for a container.</summary>
    /// <exception cref="ArgumentOutOfRangeException">An unknown container (a bug).</exception>
    private string RootFor(StorageContainer container) => container switch
    {
        StorageContainer.Public => PublicRoot,
        StorageContainer.Private => PrivateRoot,
        _ => throw new ArgumentOutOfRangeException(nameof(container), container, null)
    };

    /// <summary>True when <paramref name="folder"/> is somewhere below <paramref name="parent"/>.</summary>
    private static bool IsInside(string folder, string parent) =>
        folder.StartsWith(parent.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, PathComparison);

    /// <summary>"…/wwwroot/uploads" → "/uploads".</summary>
    private static string ToRequestPath(string webRoot, string folder) =>
        "/" + Path.GetRelativePath(webRoot, folder).Replace(Path.DirectorySeparatorChar, '/');
}