using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Presentation.Settings.Authentication;

/// <summary>
/// Registers "Continue with Google" next to JWT bearer. Called from <see cref="AuthenticationSetup"/>.
/// <list type="number">
///   <item><description><b>Settings:</b> binds and checks <see cref="ExternalAuthSettings"/> at start-up (always, so
///   the endpoints can tell the app whether Google is on).</description></item>
///   <item><description><b>Off without keys:</b> when either Google key is empty, nothing else is registered and the
///   API runs exactly as before.</description></item>
///   <item><description><b>External cookie:</b> a short-lived, encrypted cookie that only carries Google's answer from
///   <see cref="CallbackPath"/> to the API's callback endpoint, then is deleted. JWT bearer stays the default scheme;
///   no cookie ever signs anyone in to the API.</description></item>
///   <item><description><b>Google:</b> the OAuth sign-in itself; a cancel or failure on Google's side sends the
///   browser back to the app with an error instead of showing an error page.</description></item>
/// </list>
/// </summary>
/// <remarks>The values are read when the app starts: changing a Google key needs a restart (a redeploy does that).</remarks>
public static class GoogleAuthSetup
{
    /// <summary>Where Google sends the browser back to the API. Must match the redirect URI registered in Google Cloud
    /// (the API's address + this path).</summary>
    public const string CallbackPath = "/signin-google";

    /// <summary>The scheme of the short-lived cookie that holds Google's answer for one hop.</summary>
    public const string ExternalCookieScheme = "Phloem.External";

    /// <summary>The claim saying whether Google has confirmed the person owns their email ("true" / "false").</summary>
    public const string EmailVerifiedClaim = "email_verified";

    /// <summary>Key in the sign-in properties that carries the app's country through Google and back.</summary>
    public const string CountryItem = "phloem.country";

    /// <summary>The external cookie's name.</summary>
    private const string ExternalCookieName = "phloem.external";

    /// <summary>How long the external cookie lives: just long enough to finish the sign-in.</summary>
    private static readonly TimeSpan ExternalCookieLifetime = TimeSpan.FromMinutes(10);

    /// <summary>Adds the Google sign-in when both keys are set.</summary>
    /// <param name="builder">The authentication builder (JWT bearer already added).</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static AuthenticationBuilder AddPhloemGoogle(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        // 1. Settings (always: the endpoints read IsGoogleEnabled)
        var section = configuration.GetSection(ExternalAuthSettings.SectionName);
        builder.Services.AddOptions<ExternalAuthSettings>()
               .Bind(section)
               .ValidateDataAnnotations()
               .Validate(s => Uri.TryCreate(s.AppCallbackUrl, UriKind.Absolute, out _),
                         "Authentication:AppCallbackUrl must be a full address, e.g. \"phloem://auth\".")
               .ValidateOnStart();

        // 2. Off without keys
        var settings = section.Get<ExternalAuthSettings>() ?? new ExternalAuthSettings();
        if (!settings.IsGoogleEnabled) return builder;

        // 3. External cookie: one hop only
        builder.AddCookie(ExternalCookieScheme, options =>
        {
            options.Cookie.Name = ExternalCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = ExternalCookieLifetime;
            options.SlidingExpiration = false;
        });

        // 4. Google
        builder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = ExternalCookieScheme;
            options.ClientId = settings.Google.ClientId;
            options.ClientSecret = settings.Google.ClientSecret;
            options.CallbackPath = CallbackPath;
            options.SaveTokens = false;   // Phloem never calls Google's APIs, so it keeps none of Google's tokens

            // Google names this "email_verified" (userinfo v3) or "verified_email" (v2); keep whichever arrives
            options.ClaimActions.MapJsonKey(EmailVerifiedClaim, "email_verified");
            options.ClaimActions.MapJsonKey(EmailVerifiedClaim, "verified_email");

            // Cancelled or failed on Google's side: back to the app with an error, never an error page
            options.Events.OnAccessDenied = context => BackToAppAsync(context, settings, null);
            options.Events.OnRemoteFailure = context => BackToAppAsync(context, settings, context.Failure);
        });

        return builder;
    }

    /// <summary>Ends a failed Google round trip by sending the browser back to the app with
    /// <see cref="AuthErrors.ExternalLoginFailed"/>.</summary>
    /// <param name="context">The failed sign-in.</param>
    /// <param name="settings">The section (the app's address).</param>
    /// <param name="failure">What went wrong, for the log; null when the person simply said no.</param>
    /// <returns>A completed task.</returns>
    private static Task BackToAppAsync(HandleRequestContext<RemoteAuthenticationOptions> context,
                                       ExternalAuthSettings settings, Exception? failure)
    {
        if (failure is not null)
            context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                   .CreateLogger(typeof(GoogleAuthSetup))
                   .LogWarning(failure, "Google sign-in failed on the way back from Google.");

        context.Response.Redirect(settings.AppErrorLink(AuthErrors.ExternalLoginFailed));
        context.HandleResponse();   // stop here: no exception, no error page
        return Task.CompletedTask;
    }
}
