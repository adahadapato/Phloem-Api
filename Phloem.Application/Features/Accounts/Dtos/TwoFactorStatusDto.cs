namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>Account → Security: is two-factor sign-in on, and how many recovery codes are left.</summary>
/// <param name="IsEnabled">True if two-factor sign-in is on.</param>
/// <param name="RecoveryCodesLeft">Unused recovery codes (suggest new ones when this gets low).</param>
/// <param name="IsRequired">True for admins: the app hides "Switch off" and nags until it's on.</param>
public sealed record TwoFactorStatusDto(bool IsEnabled, int RecoveryCodesLeft, bool IsRequired);