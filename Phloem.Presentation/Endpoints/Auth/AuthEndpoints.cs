using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Options;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authentication;

namespace Phloem.Presentation.Endpoints.Auth;

/// <summary>
/// <c>/api/auth</c>: getting in and out. Every endpoint is open (no token needed); each one sends its command through
/// MediatR and returns the <c>Result</c> as HTTP.
/// </summary>
/// <remarks>
/// <para>Login, register, 2FA and refresh all return <see cref="AuthTokensDto"/>: either signed in (tokens) or
/// <c>requiresTwoFactor</c> with a challenge token for <c>/login/two-factor</c>.</para>
/// <para>Logout, forgot-password and reset-password return 204. Forgot-password ALWAYS returns 204, so it can't be used
/// to find out which emails have accounts.</para>
/// <para><b>Google</b> (section 6) is an extra way in; email and password stay exactly as they are. The app opens
/// <c>/external/google?country=…</c> in its sign-in sheet; the API ends at <c>phloem://auth?code=…</c>; the app swaps
/// the code at <c>/external/exchange</c> for the same <see cref="AuthTokensDto"/> as a password login.</para>
/// </remarks>
public static class AuthEndpoints
{
    /// <summary>Maps the auth endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder api)
    {
        var auth = api.MapGroup("/auth")
                      .WithTags("Auth")
                      .AllowAnonymous();

        // 1. Sign-up
        auth.MapPost("/register", async (RegisterCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("Register")
            .WithSummary("Create an account and sign in")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        // 2. Sign-in
        auth.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("Login")
            .WithSummary("Sign in with email and password (may ask for a 2FA code)")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        auth.MapPost("/login/two-factor", async (CompleteTwoFactorLoginCommand command, ISender sender,
                                                CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("CompleteTwoFactorLogin")
            .WithSummary("Finish sign-in with an authenticator code or a recovery code")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        // 3. Sessions
        auth.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("RefreshToken")
            .WithSummary("Swap a refresh token for a new pair (stay signed in)")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem();

        auth.MapPost("/logout", async (RevokeTokenCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("Logout")
            .WithSummary("Sign out: the refresh token stops working")
            .Produces(StatusCodes.Status204NoContent);

        // 4. Password reset
        auth.MapPost("/forgot-password", async (ForgotPasswordCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("ForgotPassword")
            .WithSummary("Email a password-reset link (always succeeds)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        auth.MapPost("/reset-password", async (ResetPasswordCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("ResetPassword")
            .WithSummary("Choose a new password using the emailed link")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        // 5. Email confirmation (the website's /confirm-email page posts the link's values here)
        auth.MapPost("/confirm-email", async (ConfirmEmailCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
                .WithName("ConfirmEmail")
                .WithSummary("Confirm the email address using the emailed link")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesValidationProblem();

        // 6. Google sign-in (switched off until Authentication:Google has keys)
        auth.MapGet("/external/providers", (IOptions<ExternalAuthSettings> options) =>
                Results.Ok(new ExternalProvidersResponse(Google: options.Value.IsGoogleEnabled)))
            .WithName("GetExternalProviders")
            .WithSummary("Which \"Continue with …\" buttons the app should show")
            .Produces<ExternalProvidersResponse>();

        auth.MapGet("/external/google", StartGoogleSignIn)
            .WithName("StartGoogleSignIn")
            .WithSummary("Open in the app's sign-in sheet: goes to Google's page (404 while Google isn't set up)")
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapGet("/external/google/callback", FinishGoogleSignIn)
            .WithName(GoogleCallbackEndpointName)
            .ExcludeFromDescription();   // only Google's redirect lands here, never the app

        auth.MapPost("/external/exchange", async (ExchangeLoginCodeCommand command, ISender sender,
                                                 CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("ExchangeLoginCode")
            .WithSummary("Swap the one-time code from phloem://auth?code=… for a session (may ask for a 2FA code)")
            .Produces<AuthTokensDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return api;
    }

    // ───────────────────────────── Google sign-in ─────────────────────────────

    /// <summary>Route name of the callback, so its path is never typed twice.</summary>
    private const string GoogleCallbackEndpointName = "FinishGoogleSignIn";

    /// <summary>
    /// <c>GET /api/auth/external/google?country=Ghana</c>: sends the browser to Google's page. The country (from the
    /// app's flag selector) travels with the sign-in and is used only if a new account is created.
    /// </summary>
    /// <param name="country">The country picked in the app.</param>
    /// <param name="http">The request (to build the callback path).</param>
    /// <param name="links">Builds the callback path from its route name.</param>
    /// <param name="options">The <c>Authentication</c> section.</param>
    /// <returns>A redirect to Google; or 404 <see cref="AuthErrors.ExternalLoginUnavailable"/> while Google isn't set
    /// up.</returns>
    private static IResult StartGoogleSignIn(Country country, HttpContext http, LinkGenerator links,
                                             IOptions<ExternalAuthSettings> options)
    {
        if (!options.Value.IsGoogleEnabled) return AuthErrors.ExternalLoginUnavailable.ToProblem();

        var callbackPath = links.GetPathByName(http, GoogleCallbackEndpointName)
                           ?? throw new InvalidOperationException("The Google callback endpoint isn't mapped.");

        var properties = new AuthenticationProperties { RedirectUri = callbackPath };
        properties.Items[GoogleAuthSetup.CountryItem] = country.ToString();

        return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
    }

    /// <summary>
    /// <c>GET /api/auth/external/google/callback</c>: Google has answered (through <see cref="GoogleAuthSetup.CallbackPath"/>).
    /// Finds / links / creates the account and sends the browser back to the app with a one-time code. ALWAYS ends
    /// with a redirect to the app, so the person never sees an API error page.
    /// </summary>
    /// <param name="http">The request (holds Google's answer in the external cookie).</param>
    /// <param name="sender">MediatR.</param>
    /// <param name="options">The <c>Authentication</c> section (the app's address).</param>
    /// <param name="ct">Cancels the work.</param>
    /// <returns><c>phloem://auth?code=…</c>, or <c>phloem://auth?error=…&amp;message=…</c>.</returns>
    private static async Task<IResult> FinishGoogleSignIn(HttpContext http, ISender sender,
                                                          IOptions<ExternalAuthSettings> options, CancellationToken ct)
    {
        var settings = options.Value;
        if (!settings.IsGoogleEnabled)
            return Results.Redirect(settings.AppErrorLink(AuthErrors.ExternalLoginUnavailable));

        // Read Google's answer, then delete the cookie: it was only needed for this one hop
        var google = await http.AuthenticateAsync(GoogleAuthSetup.ExternalCookieScheme);
        await http.SignOutAsync(GoogleAuthSetup.ExternalCookieScheme);

        var command = google.Succeeded ? ToCommand(google.Principal, google.Properties) : null;
        if (command is null) return Results.Redirect(settings.AppErrorLink(AuthErrors.ExternalLoginFailed));

        try
        {
            var code = await sender.Send(command, ct);
            return Results.Redirect(code.IsSuccess
                ? settings.AppLink(("code", code.Value))
                : settings.AppErrorLink(code.Error));
        }
        catch (ValidationException)   // Google's values didn't pass the command's checks
        {
            return Results.Redirect(settings.AppErrorLink(AuthErrors.ExternalLoginFailed));
        }
    }

    /// <summary>Turns Google's answer into the plain command Application understands.</summary>
    /// <param name="google">Who Google says the person is (claims).</param>
    /// <param name="properties">The sign-in's properties (they carry the app's country).</param>
    /// <returns>The command; or null when Google's answer lacks an Id, an email or the country.</returns>
    private static SignInWithExternalLoginCommand? ToCommand(ClaimsPrincipal? google,
                                                             AuthenticationProperties? properties)
    {
        var key = google?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = google?.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(email)) return null;

        if (properties is null ||
            !properties.Items.TryGetValue(GoogleAuthSetup.CountryItem, out var countryText) ||
            !Enum.TryParse<Country>(countryText, out var country))
            return null;

        var emailVerified = google!.FindAll(GoogleAuthSetup.EmailVerifiedClaim)
                                   .Any(c => bool.TryParse(c.Value, out var verified) && verified);

        return new SignInWithExternalLoginCommand(
            Provider: GoogleDefaults.AuthenticationScheme,
            ProviderKey: key,
            Email: email,
            EmailVerified: emailVerified,
            FirstName: google.FindFirstValue(ClaimTypes.GivenName),
            LastName: google.FindFirstValue(ClaimTypes.Surname),
            Country: country);
    }
}