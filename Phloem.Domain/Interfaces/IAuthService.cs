using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Getting in: sign-up, login (with optional 2FA), tokens, email confirmation, password reset.
/// Implemented in Infrastructure using ASP.NET Core Identity + JWT.
/// Failures (wrong password, expired token…) are returned as AuthErrors, never thrown.
/// </summary>
public interface IAuthService
{
    /// <summary>Sign-up: creates the login account and sends the confirmation email. Returns the new user's Id.</summary>
    Task<Result<Guid>> RegisterAsync(string email, string password, string? phoneNumber,
                                     CancellationToken ct = default);

    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default);

    /// <summary>
    /// Checks email + password. If 2FA is on, returns RequiresTwoFactor with a challenge token
    /// and NO access / refresh tokens until the second step succeeds.
    /// </summary>
    Task<Result<LoginOutcome>> LoginAsync(string email, string password, CancellationToken ct = default);

    /// <summary>Second step of a 2FA login, with an authenticator code or a recovery code.</summary>
    Task<Result<AuthTokens>> CompleteTwoFactorLoginAsync(string challengeToken, string code, bool isRecoveryCode,
                                                         CancellationToken ct = default);

    /// <summary>Swaps a valid refresh token for a new access + refresh pair (the app stays signed in).</summary>
    Task<Result<AuthTokens>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Log out: the refresh token can't be used again.</summary>
    Task<Result> RevokeTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>"Forgot password": always succeeds, even for unknown emails, so attackers can't probe accounts.</summary>
    Task<Result> SendPasswordResetAsync(string email, CancellationToken ct = default);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default);

    // ───────────────────────────── Outside sign-in (Google) ─────────────────────────────

    /// <summary>
    /// Finds the Phloem account for someone the provider has just signed in: first by the provider's login, then by
    /// email. A match by email links the provider's login to that account, so next time it is found directly.
    /// </summary>
    /// <param name="identity">Who the provider says the person is.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The account's Id; or <c>AuthErrors.NoAccountForExternalLogin</c> when there is no account yet (the
    /// caller then creates one with <see cref="RegisterExternalAsync"/>); or <c>AuthErrors.ExternalEmailNotVerified</c>
    /// when the email belongs to an account but the provider hasn't confirmed the person owns it.</returns>
    /// <remarks>Linking also marks the account's email as confirmed: the provider has just proved it.</remarks>
    Task<Result<Guid>> FindExternalLoginAsync(ExternalIdentity identity, CancellationToken ct = default);

    /// <summary>
    /// Sign-up through a provider: creates a login account WITHOUT a password (email already confirmed, the
    /// provider's login linked, role <c>Customer</c>). Runs inside the caller's transaction, which also creates the
    /// UserProfile with the same Id.
    /// </summary>
    /// <param name="identity">Who the provider says the person is (its email must be verified).</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The new user's Id; or <c>AuthErrors.ExternalEmailNotVerified</c> / <c>AuthErrors.EmailTaken</c>.</returns>
    /// <remarks>The person can add a password later with "Forgot password", and then sign in either way.</remarks>
    Task<Result<Guid>> RegisterExternalAsync(ExternalIdentity identity, CancellationToken ct = default);

    /// <summary>
    /// Signs in an account the provider has just vouched for (no password step). Same checks as a password login
    /// after the password: lockout, deactivated, suspended, then 2FA.
    /// </summary>
    /// <param name="userId">The account, from <see cref="FindExternalLoginAsync"/> or
    /// <see cref="RegisterExternalAsync"/>.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Signed in (tokens), or RequiresTwoFactor with a challenge token, exactly like
    /// <see cref="LoginAsync"/>; or the same errors.</returns>
    Task<Result<LoginOutcome>> SignInExternalAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>A signed-in session: short-lived access token + long-lived refresh token.</summary>
public sealed record AuthTokens(
    string AccessToken, DateTime AccessTokenExpiresAt,
    string RefreshToken, DateTime RefreshTokenExpiresAt,
    Guid UserId, IReadOnlyCollection<string> Roles);

/// <summary>Login either finishes (Tokens) or needs a 2FA code first (ChallengeToken).</summary>
public sealed record LoginOutcome(AuthTokens? Tokens, string? ChallengeToken)
{
    public bool RequiresTwoFactor => ChallengeToken is not null;
}