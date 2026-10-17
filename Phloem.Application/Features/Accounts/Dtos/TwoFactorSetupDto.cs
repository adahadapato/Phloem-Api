namespace Phloem.Application.Features.Accounts.Dtos;

/// <summary>
/// Security → "Set up two-factor": what the authenticator app needs. Shown ONCE during setup.
/// </summary>
/// <param name="ManualEntryKey">The secret key in groups of 4 ("ABCD EFGH …") for typing in by hand.</param>
/// <param name="AuthenticatorUri">The otpauth:// link the app turns into a QR code.</param>
public sealed record TwoFactorSetupDto(string ManualEntryKey, string AuthenticatorUri);