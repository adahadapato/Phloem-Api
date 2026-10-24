using Phloem.Domain.Constants;

namespace Phloem.Presentation.Settings.Authentication;

/// <summary>
/// The <c>"Authentication:Google"</c> sub-section: Google's OAuth client (type "Web application"), created by the app's
/// owners in their Google Cloud account. Keys are listed in <see cref="AppSettings.Authentication.Google"/>.
/// </summary>
/// <remarks>
/// <para>Both values stay empty <c>""</c> in appsettings.json, so CI/CD can map them
/// (<c>Authentication__Google__ClientId</c>, <c>Authentication__Google__ClientSecret</c>). While either is empty,
/// Google sign-in is switched off and the API runs exactly as before.</para>
/// <para>The client's redirect URI in Google Cloud must be the API's address + <see cref="GoogleAuthSetup.CallbackPath"/>,
/// e.g. <c>https://phloemapi.phloemmarket.com/signin-google</c>.</para>
/// </remarks>
public sealed class GoogleAuthSettings
{
    /// <summary>The OAuth client Id ("….apps.googleusercontent.com"). Empty = Google switched off.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>The OAuth client secret. SECRET: User Secrets / GitHub Secrets only. Empty = Google switched off.</summary>
    public string ClientSecret { get; set; } = string.Empty;
}
