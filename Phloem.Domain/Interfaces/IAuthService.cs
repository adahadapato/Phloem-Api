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