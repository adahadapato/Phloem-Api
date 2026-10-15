using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// The signed-in user's LOGIN account: email, phone, password, two-factor, deleting the login.
/// Name, country, photo and addresses are NOT here; they live on the UserProfile entity.
/// Implemented in Infrastructure using ASP.NET Core Identity.
/// </summary>
public interface IAccountService
{
    /// <summary>For "leave blank to use your personal email / phone" on Seller setup.</summary>
    Task<Result<AccountContact>> GetContactAsync(Guid userId, CancellationToken ct = default);

    Task<Result> UpdatePhoneNumberAsync(Guid userId, string? phoneNumber, CancellationToken ct = default);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword,
                                     CancellationToken ct = default);

    // ---------- Two-factor (recommended for admins and sellers) ----------
    Task<Result<TwoFactorStatus>> GetTwoFactorStatusAsync(Guid userId, CancellationToken ct = default);
    Task<Result<TwoFactorSetup>> GetTwoFactorSetupAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Turns 2FA on after checking a code from the authenticator app; returns recovery codes.</summary>
    Task<Result<IReadOnlyCollection<string>>> EnableTwoFactorAsync(Guid userId, string verificationCode,
                                                                   CancellationToken ct = default);
    Task<Result> DisableTwoFactorAsync(Guid userId, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<string>>> GenerateRecoveryCodesAsync(Guid userId, CancellationToken ct = default);

    // ---------- Delete account ----------
    /// <summary>Step 1 of "Delete my account": confirms the password before anything is removed.</summary>
    Task<Result> VerifyPasswordAsync(Guid userId, string password, CancellationToken ct = default);

    /// <summary>Wipes email / phone / password and revokes all tokens, so the login can never be used again.</summary>
    Task<Result> AnonymiseAsync(Guid userId, CancellationToken ct = default);
}

public sealed record AccountContact(string Email, string? PhoneNumber, bool EmailConfirmed);

public sealed record TwoFactorStatus(bool IsEnabled, int RecoveryCodesLeft);

/// <summary>What the authenticator app needs: the shared key, and a QR-code URI to scan.</summary>
public sealed record TwoFactorSetup(string SharedKey, string AuthenticatorUri);