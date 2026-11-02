using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Auth.Dtos;

/// <summary>
/// What Login, Register, Refresh and 2FA all return, so the app handles ONE shape.
/// Either signed in (tokens + who) or waiting for a 2FA code (RequiresTwoFactor + challenge token).
/// Full profile (photo, addresses, notifications) comes from GET /me (GetMyProfileQuery).
/// </summary>
/// <remarks>
/// <para><b>Permissions / Countries</b> (staff only, added at the END so older app versions ignore them): what the
/// backend has decided this person may do and where. "*" means every permission (Admin) or every country. The
/// website and app use them only to show or hide menus; every API call is still checked by the backend.</para>
/// </remarks>
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
    string? TwoFactorChallengeToken = null,
    IReadOnlyCollection<string>? Permissions = null,
    IReadOnlyCollection<string>? Countries = null)
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
        email,
        Permissions: tokens.Permissions,
        Countries: tokens.Countries);

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
        TwoFactorChallengeToken: challengeToken,
        Permissions: [],
        Countries: []);
}