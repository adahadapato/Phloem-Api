using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.WebUtilities;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Presentation.Settings.Authentication;

/// <summary>
/// The <c>"Authentication"</c> section of appsettings.json: outside sign-in (Google). Keys are listed in
/// <see cref="AppSettings.Authentication"/>.
/// </summary>
/// <remarks>
/// <para><b>Off until the keys exist:</b> Google is registered only when <see cref="GoogleAuthSettings.ClientId"/> AND
/// <see cref="GoogleAuthSettings.ClientSecret"/> are filled in (<see cref="IsGoogleEnabled"/>). The owners create them in
/// their Google Cloud account; they go into User Secrets (development) and GitHub Secrets (production), never into
/// the repo.</para>
/// <para><c>LoginCodeSeconds</c> in the same section is read by Infrastructure (<c>LoginCodeSettings</c>).</para>
/// </remarks>
public sealed class ExternalAuthSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Authentication.Section;

    /// <summary>
    /// Where the browser goes back to the app after Google, e.g. "phloem://auth". The API adds <c>?code=…</c>
    /// (success) or <c>?error=…&amp;message=…</c> (failure). Never taken from the request, so the sign-in endpoints
    /// can't be used to send people to other sites.
    /// </summary>
    [Required(ErrorMessage = "Authentication:AppCallbackUrl is empty (e.g. \"phloem://auth\").")]
    public string AppCallbackUrl { get; set; } = "phloem://auth";

    /// <summary>Google's OAuth client.</summary>
    [Required]
    public GoogleAuthSettings Google { get; set; } = new();

    /// <summary>True when both Google keys are filled in, so "Continue with Google" works.</summary>
    public bool IsGoogleEnabled =>
        !string.IsNullOrWhiteSpace(Google.ClientId) && !string.IsNullOrWhiteSpace(Google.ClientSecret);

    /// <summary>The link back into the app, with the given values added to its query string.</summary>
    /// <param name="query">Name / value pairs, e.g. ("code", "…"). Values are URL-encoded.</param>
    /// <returns>For example <c>phloem://auth?code=abc</c>.</returns>
    public string AppLink(params (string Name, string Value)[] query) =>
        QueryHelpers.AddQueryString(AppCallbackUrl, query.ToDictionary(q => q.Name, q => (string?)q.Value));

    /// <summary>The link back into the app for a failure: <c>?error=&lt;code&gt;&amp;message=&lt;text&gt;</c>.</summary>
    /// <param name="error">What went wrong (its code lets the app react; its message can be shown as it is).</param>
    /// <returns>For example <c>phloem://auth?error=Auth.ExternalLoginFailed&amp;message=…</c>.</returns>
    public string AppErrorLink(Error error) => AppLink(("error", error.Code), ("message", error.Message));
}
