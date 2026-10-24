using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Infrastructure.Settings.Application;
using Phloem.Infrastructure.Settings.Identity;
using Phloem.Persistence.Configurations.Identity;
using System.Text;
using System.Text.Encodings.Web;

namespace Phloem.Infrastructure.Services.Identity;

/// <summary>
/// <see cref="IAuthService"/> with ASP.NET Core Identity and JWT: sign-up, sign-in (with 2FA), sessions, email
/// confirmation and password reset.
/// </summary>
/// <remarks>
/// <para><b>Sessions:</b> an access token (JWT, short) plus a refresh token (random, long; only its hash is stored).
/// Every refresh rotates the refresh token. A revoked token presented again means it was copied, so every session of
/// that user is ended.</para>
/// <para><b>Sign-in checks, in order:</b> lockout → password → deactivated → suspended → email confirmed (if
/// required) → 2FA. "Deactivated" and "suspended" are only revealed to someone who knows the password.</para>
/// <para><b>Suspended</b> people (UserProfile.IsSuspendedAt) can't sign in, and renewing a session fails, so an app
/// that was signed in is signed out at the next renewal (the access token is short-lived).</para>
/// <para><b>Google sign-in</b> replaces only the password step: the account is found by its linked login or its
/// email, or created without a password, and then the same checks run (lockout → deactivated → suspended → 2FA).</para>
/// <para><b>Emails</b> never fail the request: a sending error is logged, and the user can ask again.</para>
/// <para><b>Saving:</b> UserManager saves its own changes; refresh tokens and <c>LastLoginAt</c> are saved through
/// <see cref="IUnitOfWork"/>. Both use the same scoped database context.</para>
/// </remarks>
internal sealed class AuthService(
    UserManager<ApplicationUser> users,
    IRefreshTokenRepository refreshTokens,
    IUserProfileRepository profiles,
    IUnitOfWork unitOfWork,
    JwtTokenGenerator tokenGenerator,
    TwoFactorChallenge challenges,
    IEmailSender emailSender,
    IOptions<IdentitySettings> identityOptions,
    IOptions<ApplicationSettings> appOptions,
    TimeProvider clock,
    ILogger<AuthService> logger) : IAuthService
{
    // ───────────────────────────── 1. Sign-up ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>Every new account gets the <see cref="Roles.Customer"/> role. Runs inside the caller's transaction
    /// (RegisterCommandHandler), so a failure later rolls the account back.</remarks>
    public async Task<Result<Guid>> RegisterAsync(string email, string password, string? phoneNumber,
                                                  CancellationToken ct = default)
    {
        if (await users.FindByEmailAsync(email) is not null) return AuthErrors.EmailTaken;

        var user = new ApplicationUser { UserName = email, Email = email, PhoneNumber = phoneNumber };

        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded) return IdentityErrors.ToError(created);

        var role = await users.AddToRoleAsync(user, Roles.Customer);
        if (!role.Succeeded) return IdentityErrors.ToError(role);

        await SendConfirmationEmailAsync(user, ct);
        return user.Id;
    }

    /// <inheritdoc />
    public async Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || DecodeToken(token) is not { } decoded) return AuthErrors.InvalidToken;

        var confirmed = await users.ConfirmEmailAsync(user, decoded);
        return confirmed.Succeeded ? Result.Success() : AuthErrors.InvalidToken;
    }

    // ───────────────────────────── 2. Sign-in ─────────────────────────────

    /// <inheritdoc />
    public async Task<Result<LoginOutcome>> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null) return AuthErrors.InvalidCredentials;

        var passwordCheck = await CheckPasswordAsync(user, password);
        if (passwordCheck.IsFailure) return passwordCheck.Error;

        var allowed = await CheckCanSignInAsync(user, ct);
        if (allowed.IsFailure) return allowed.Error;

        // Password right, but a 2FA code is needed before any real tokens
        if (user.TwoFactorEnabled)
            return new LoginOutcome(null, challenges.Create(user));

        var session = await StartSessionAsync(user, isNewSignIn: true);
        await unitOfWork.SaveChangesAsync(ct);

        return new LoginOutcome(session.Tokens, null);
    }

    /// <inheritdoc />
    public async Task<Result<AuthTokens>> CompleteTwoFactorLoginAsync(string challengeToken, string code,
                                                                      bool isRecoveryCode,
                                                                      CancellationToken ct = default)
    {
        if (!challenges.TryRead(challengeToken, out var userId, out var stamp)) return AuthErrors.InvalidToken;

        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null || !user.TwoFactorEnabled || user.SecurityStamp != stamp) return AuthErrors.InvalidToken;

        if (await users.IsLockedOutAsync(user)) return AuthErrors.LockedOut;

        var cleanCode = code.Replace(" ", string.Empty);
        var valid = isRecoveryCode
            ? (await users.RedeemTwoFactorRecoveryCodeAsync(user, cleanCode)).Succeeded
            : await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider,
                                                    cleanCode.Replace("-", string.Empty));

        if (!valid)
        {
            await users.AccessFailedAsync(user);   // wrong codes count towards the lockout too
            return await users.IsLockedOutAsync(user) ? AuthErrors.LockedOut : AuthErrors.InvalidTwoFactorCode;
        }

        if (user.AccessFailedCount > 0) await users.ResetAccessFailedCountAsync(user);

        var allowed = await CheckCanSignInAsync(user, ct);
        if (allowed.IsFailure) return allowed.Error;

        var session = await StartSessionAsync(user, isNewSignIn: true);
        await unitOfWork.SaveChangesAsync(ct);

        return session.Tokens;
    }

    // ───────────────────────────── 3. Sessions ─────────────────────────────

    /// <inheritdoc />
    public async Task<Result<AuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var now = Now;
        var stored = await refreshTokens.GetByTokenAsync(refreshToken, ct);
        if (stored is null) return AuthErrors.InvalidToken;

        // A revoked token used again: it was copied. End every session of this user.
        if (stored.RevokedAtUtc is not null)
        {
            logger.LogWarning("A revoked refresh token was used again for user {UserId}; ending all their sessions.",
                              stored.UserId);
            await RevokeAllAsync(stored.UserId, now, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return AuthErrors.InvalidToken;
        }

        if (!stored.IsActive(now)) return AuthErrors.InvalidToken;   // expired

        var user = await users.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            stored.Revoke(now);
            await unitOfWork.SaveChangesAsync(ct);
            return user is null ? AuthErrors.InvalidToken : AuthErrors.AccountDeactivated;
        }

        // Suspended since signing in: no new session (the app shows the message and signs out)
        if (await SuspensionAsync(user.Id, now, ct) is { } suspended)
        {
            stored.Revoke(now);
            await unitOfWork.SaveChangesAsync(ct);
            return suspended;
        }

        // Rotate: new pair (with the user's CURRENT roles), old token points at its replacement
        var session = await StartSessionAsync(user, isNewSignIn: false);
        stored.Revoke(now, session.Row.Id);
        await unitOfWork.SaveChangesAsync(ct);

        return session.Tokens;
    }

    /// <inheritdoc />
    /// <remarks>Signing out twice, or with an unknown token, still succeeds.</remarks>
    public async Task<Result> RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await refreshTokens.GetByTokenAsync(refreshToken, ct);
        if (stored is null) return Result.Success();

        stored.Revoke(Now);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ───────────────────────────── 4. Password reset ─────────────────────────────

    /// <inheritdoc />
    public async Task<Result> SendPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.Email))
            return Result.Success();   // same answer as for a real account

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var link = BuildLink(identityOptions.Value.Links.ResetPasswordPath,
                             ("email", user.Email), ("token", EncodeToken(token)));

        await SendSafelyAsync(user.Email, "Reset your Phloem password",
            $"""
         <p>Hello,</p>
         <p>We received a request to reset your Phloem password.</p>
         <p><a href="{HtmlEncoder.Default.Encode(link)}">Choose a new password</a></p>
         <p>If you didn't ask for this, you can ignore this email: your password won't change.</p>
         """, ct);

        return Result.Success();
    }

    /// <inheritdoc />
    /// <remarks>On success: the email counts as confirmed (the link proved they own it), any lockout is lifted, and
    /// every session is ended, so a thief who knew the old password is signed out.</remarks>
    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword,
                                                 CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        if (user is null || DecodeToken(token) is not { } decoded) return AuthErrors.InvalidToken;

        var reset = await users.ResetPasswordAsync(user, decoded, newPassword);
        if (!reset.Succeeded)
            return reset.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken))
                ? AuthErrors.InvalidToken
                : IdentityErrors.ToError(reset);

        user.EmailConfirmed = true;
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        await RevokeAllAsync(user.Id, Now, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ───────────────────────────── 5. Outside sign-in (Google) ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// <para>Order: the provider's login (already linked) → the email (link it now). A login that is already linked
    /// is trusted even if the provider no longer reports the email as verified: the person proved it when linking.</para>
    /// <para>Linking sets <c>EmailConfirmed</c> in the same save as the new login row.</para>
    /// </remarks>
    public async Task<Result<Guid>> FindExternalLoginAsync(ExternalIdentity identity, CancellationToken ct = default)
    {
        // 1. Already linked
        var linkedUser = await users.FindByLoginAsync(identity.Provider, identity.ProviderKey);
        if (linkedUser is not null) return linkedUser.Id;

        // 2. Same email as an existing account: link, but only if the provider confirmed the email
        var user = await users.FindByEmailAsync(identity.Email.Trim());
        if (user is null) return AuthErrors.NoAccountForExternalLogin;
        if (!identity.EmailVerified) return AuthErrors.ExternalEmailNotVerified;

        user.EmailConfirmed = true;   // saved by AddLoginAsync below
        var linked = await users.AddLoginAsync(user, ToLoginInfo(identity));
        if (!linked.Succeeded) return IdentityErrors.ToError(linked);

        logger.LogInformation("Linked a {Provider} login to user {UserId}.", identity.Provider, user.Id);
        return user.Id;
    }

    /// <inheritdoc />
    /// <remarks>Every new account gets the <see cref="Roles.Customer"/> role, like a password sign-up. No confirmation
    /// email is sent: the provider has already confirmed the address.</remarks>
    public async Task<Result<Guid>> RegisterExternalAsync(ExternalIdentity identity, CancellationToken ct = default)
    {
        if (!identity.EmailVerified) return AuthErrors.ExternalEmailNotVerified;

        var email = identity.Email.Trim();
        if (await users.FindByEmailAsync(email) is not null) return AuthErrors.EmailTaken;

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };

        var created = await users.CreateAsync(user);   // no password: they can add one with "Forgot password"
        if (!created.Succeeded) return IdentityErrors.ToError(created);

        var role = await users.AddToRoleAsync(user, Roles.Customer);
        if (!role.Succeeded) return IdentityErrors.ToError(role);

        var linked = await users.AddLoginAsync(user, ToLoginInfo(identity));
        if (!linked.Succeeded) return IdentityErrors.ToError(linked);

        return user.Id;
    }

    /// <inheritdoc />
    public async Task<Result<LoginOutcome>> SignInExternalAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is null) return AuthErrors.UserNotFound;

        // A lockout from wrong passwords also blocks Google until it ends (same account, same rule)
        if (await users.IsLockedOutAsync(user)) return AuthErrors.LockedOut;

        var allowed = await CheckCanSignInAsync(user, ct);
        if (allowed.IsFailure) return allowed.Error;

        // Google proved who they are, but a 2FA code is still needed before any real tokens
        if (user.TwoFactorEnabled)
            return new LoginOutcome(null, challenges.Create(user));

        var session = await StartSessionAsync(user, isNewSignIn: true);
        await unitOfWork.SaveChangesAsync(ct);

        return new LoginOutcome(session.Tokens, null);
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>The provider's login as Identity stores it (table AspNetUserLogins).</summary>
    /// <param name="identity">Who the provider says the person is.</param>
    /// <returns>Provider name, the provider's Id for the person, and the display name (the provider's name).</returns>
    private static UserLoginInfo ToLoginInfo(ExternalIdentity identity) =>
        new(identity.Provider, identity.ProviderKey, identity.Provider);

    /// <summary>The current time (UTC), from the app's clock.</summary>
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    /// <summary>Checks the password, counting failures towards the lockout.</summary>
    private async Task<Result> CheckPasswordAsync(ApplicationUser user, string password)
    {
        if (await users.IsLockedOutAsync(user)) return AuthErrors.LockedOut;

        if (await users.CheckPasswordAsync(user, password))
        {
            if (user.AccessFailedCount > 0) await users.ResetAccessFailedCountAsync(user);
            return Result.Success();
        }

        await users.AccessFailedAsync(user);
        return await users.IsLockedOutAsync(user) ? AuthErrors.LockedOut : AuthErrors.InvalidCredentials;
    }

    /// <summary>Rules that apply after the password is known to be right.</summary>
    private async Task<Result> CheckCanSignInAsync(ApplicationUser user, CancellationToken ct)
    {
        if (!user.IsActive) return AuthErrors.AccountDeactivated;
        if (await SuspensionAsync(user.Id, Now, ct) is { } suspended) return suspended;
        if (identityOptions.Value.RequireConfirmedEmail && !user.EmailConfirmed) return AuthErrors.EmailNotConfirmed;
        return Result.Success();
    }

    /// <summary>The "your account is suspended…" error when the person is suspended right now; otherwise null.</summary>
    private async Task<Error?> SuspensionAsync(Guid userId, DateTime now, CancellationToken ct)
    {
        var profile = await profiles.GetByIdAsync(userId, ct);
        return profile is not null && profile.IsSuspendedAt(now)
            ? UserProfileErrors.Suspended(profile.SuspendedUntilUtc, profile.SuspensionReason)
            : null;
    }

    /// <summary>
    /// Makes a new access + refresh pair and adds the refresh token for saving. Does NOT save: the caller may have
    /// more to change first (e.g. revoking the old token).
    /// </summary>
    /// <param name="user">The user signing in.</param>
    /// <param name="isNewSignIn">True for password / 2FA sign-in (updates <c>LastLoginAt</c>); false for a refresh.</param>
    private async Task<Session> StartSessionAsync(ApplicationUser user, bool isNewSignIn)
    {
        var roles = (await users.GetRolesAsync(user)).ToArray();

        var access = tokenGenerator.CreateAccessToken(user, roles);
        var refresh = tokenGenerator.CreateRefreshToken();

        var row = new RefreshToken(user.Id, refresh.Value, refresh.IssuedAtUtc, refresh.ExpiresAtUtc);
        refreshTokens.Add(row);

        if (isNewSignIn) user.LastLoginAt = access.IssuedAtUtc;

        var tokens = new AuthTokens(access.Value, access.ExpiresAtUtc,
                                    refresh.Value, refresh.ExpiresAtUtc,
                                    user.Id, roles);
        return new Session(tokens, row);
    }

    /// <summary>Revokes every working refresh token of a user (not saved).</summary>
    private async Task RevokeAllAsync(Guid userId, DateTime now, CancellationToken ct)
    {
        foreach (var token in await refreshTokens.GetActiveForUserAsync(userId, now, ct))
            token.Revoke(now);
    }

    /// <summary>Sends the "confirm your email" link.</summary>
    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.Email)) return;

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var link = BuildLink(identityOptions.Value.Links.ConfirmEmailPath,
                             ("userId", user.Id.ToString()), ("token", EncodeToken(token)));

        await SendSafelyAsync(user.Email, "Confirm your Phloem email",
            $"""
         <p>Welcome to Phloem!</p>
         <p>Please confirm your email address:</p>
         <p><a href="{HtmlEncoder.Default.Encode(link)}">Confirm my email</a></p>
         <p>If you didn't create an account, you can ignore this email.</p>
         """, ct);
    }

    /// <summary>Sends an email; a failure is logged, never thrown (the user can ask again).</summary>
    private async Task SendSafelyAsync(string to, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            await emailSender.SendAsync(to, subject, htmlBody, EmailReplyTo.Support, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't send '{Subject}' to {Email}.", subject, to);
        }
    }

    /// <summary>A website link: base URL + path + query string (values URL-encoded).</summary>
    private string BuildLink(string path, params (string Name, string Value)[] query)
    {
        var url = $"{appOptions.Value.WebsiteBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        return QueryHelpers.AddQueryString(url, query.ToDictionary(q => q.Name, q => (string?)q.Value));
    }

    /// <summary>Identity tokens contain characters that break in links; Base64Url makes them link-safe.</summary>
    private static string EncodeToken(string token) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    /// <summary>Reverses <see cref="EncodeToken"/>; null if the link was damaged.</summary>
    private static string? DecodeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    /// <summary>A new session: what the app gets, and the refresh-token row that backs it.</summary>
    private sealed record Session(AuthTokens Tokens, RefreshToken Row);
}
