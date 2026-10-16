using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Auth.Dtos;

/// <summary>
/// What Login, Register, Refresh and 2FA all return, so the app handles ONE shape.
/// Either signed in (tokens + who) or waiting for a 2FA code (RequiresTwoFactor + challenge token).
/// Full profile (photo, addresses, notifications) comes from GET /me (GetMyProfileQuery).
/// </summary>
public sealed record AuthTokensDto(
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    DateTime? RefreshTokenExpiresAt,
    Guid? UserId,
    IReadOnlyCollection<string> Roles,
    string? FullName,
    string? Email,
    bool RequiresTwoFactor = false,
    string? TwoFactorChallengeToken = null)
{
    /// <summary>Signed in.</summary>
    public static AuthTokensDto SignedIn(AuthTokens tokens, string fullName, string email) => new(
        tokens.AccessToken,
        tokens.RefreshToken,
        tokens.AccessTokenExpiresAt,
        tokens.RefreshTokenExpiresAt,
        tokens.UserId,
        tokens.Roles,
        fullName,
        email);

    /// <summary>Password was right, but a 2FA code is needed (→ CompleteTwoFactorLoginCommand).</summary>
    public static AuthTokensDto TwoFactorRequired(string challengeToken) => new(
        AccessToken: null,
        RefreshToken: null,
        ExpiresAt: null,
        RefreshTokenExpiresAt: null,
        UserId: null,
        Roles: [],
        FullName: null,
        Email: null,
        RequiresTwoFactor: true,
        TwoFactorChallengeToken: challengeToken);
}