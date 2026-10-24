using Phloem.Domain.Constants;

namespace Phloem.Presentation.Settings.Cors;

/// <summary>
/// The <c>Cors</c> section: which websites may call the API from a browser. Keys: <see cref="AppSettings.Cors"/>.
/// </summary>
/// <remarks>
/// <para>Production lists the Phloem website (<c>https://phloemmarket.com</c>, with and without www); development
/// lists the local Vite address (<c>http://localhost:5173</c>). The mobile app needs no entry: it isn't a browser.</para>
/// <para>Checked when the app starts (see <see cref="CorsSetup"/>): a malformed address stops the app instead of
/// silently blocking the website.</para>
/// </remarks>
public sealed class CorsSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Cors.Section;

    /// <summary>Full addresses, e.g. "https://phloemmarket.com": scheme + host (+ port), no path, no trailing
    /// slash. Empty = no website may call the API from a browser.</summary>
    public string[] AllowedOrigins { get; set; } = [];
}
