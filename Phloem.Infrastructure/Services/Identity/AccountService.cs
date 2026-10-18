using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Interfaces.Identity;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Infrastructure.Settings.Identity;
using Phloem.Persistence.Configurations.Identity;
using System.Text;

namespace Phloem.Infrastructure.Services.Identity;

/// <summary>
/// <see cref="IAccountService"/> with ASP.NET Core Identity: the signed-in user's own login account (contact,
/// password, two-factor, deleting the login).
/// </summary>
/// <remarks>
/// <para><b>Ends every session</b> (revokes all refresh tokens) when the password changes or the account is deleted,
/// so anyone who knew the old password is signed out. The app should send the user back to sign-in after a password
/// change.</para>
/// <para>Names, photo and addresses are on <c>UserProfile</c>, not here.</para>
/// </remarks>
internal sealed class AccountService(
    UserManager<ApplicationUser> users,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork unitOfWork,
    IOptions<IdentitySettings> identityOptions,
    TimeProvider clock) : IAccountService
{
    /// <summary>Recovery codes handed out when 2FA is switched on or the codes are regenerated.</summary>
    private const int RecoveryCodeCount = 10;

    /// <summary>Domain used for deleted accounts' placeholder emails (".invalid" can never be a real address).</summary>
    private const string DeletedEmailDomain = "deleted.invalid";

    // ───────────────────────────── 1. Contact ─────────────────────────────

    /// <inheritdoc />
    public async Task<Result<AccountContact>> GetContactAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        return new AccountContact(user.Email ?? string.Empty, user.PhoneNumber, user.EmailConfirmed);
    }

    /// <inheritdoc />
    /// <remarks>Null or blank removes the phone number.</remarks>
    public async Task<Result> UpdatePhoneNumberAsync(Guid userId, string? phoneNumber, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        var phone = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        var result = await users.SetPhoneNumberAsync(user, phone);
        return result.Succeeded ? Result.Success() : IdentityErrors.ToError(result);
    }

    // ───────────────────────────── 2. Password ─────────────────────────────

    /// <inheritdoc />
    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword,
                                                  CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        var changed = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changed.Succeeded)
            return changed.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch))
                ? AuthErrors.WrongPassword
                : IdentityErrors.ToError(changed);

        await RevokeAllSessionsAsync(user.Id, ct);
        return Result.Success();
    }

    /// <inheritdoc />
    /// <remarks>Wrong passwords count towards the lockout, so this can't be used to guess passwords.</remarks>
    public async Task<Result> VerifyPasswordAsync(Guid userId, string password, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;
        if (await users.IsLockedOutAsync(user)) return AuthErrors.LockedOut;

        if (await users.CheckPasswordAsync(user, password))
        {
            if (user.AccessFailedCount > 0) await users.ResetAccessFailedCountAsync(user);
            return Result.Success();
        }

        await users.AccessFailedAsync(user);
        return await users.IsLockedOutAsync(user) ? AuthErrors.LockedOut : AuthErrors.WrongPassword;
    }

    // ───────────────────────────── 3. Two-factor ─────────────────────────────

    /// <inheritdoc />
    public async Task<Result<TwoFactorStatus>> GetTwoFactorStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        return new TwoFactorStatus(user.TwoFactorEnabled, await users.CountRecoveryCodesAsync(user));
    }

    /// <inheritdoc />
    /// <remarks>Creates the user's authenticator key the first time; afterwards returns the same key until 2FA is
    /// switched off.</remarks>
    public async Task<Result<TwoFactorSetup>> GetTwoFactorSetupAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        var key = await users.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await users.ResetAuthenticatorKeyAsync(user);
            key = await users.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrEmpty(key))
            return Error.Failure("Auth.AuthenticatorKey", "Two-factor setup couldn't start. Please try again.");

        return new TwoFactorSetup(FormatKey(key), AuthenticatorUri(user.Email ?? user.Id.ToString(), key));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<string>>> EnableTwoFactorAsync(Guid userId, string verificationCode,
                                                                               CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        var code = verificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);
        var valid = await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, code);
        if (!valid) return AuthErrors.InvalidTwoFactorCode;

        var enabled = await users.SetTwoFactorEnabledAsync(user, true);
        if (!enabled.Succeeded) return IdentityErrors.ToError(enabled);

        return await NewRecoveryCodesAsync(user);
    }

    /// <inheritdoc />
    /// <remarks>Also resets the authenticator key, so turning 2FA back on means scanning a new QR code (the old phone
    /// entry stops working).</remarks>
    public async Task<Result> DisableTwoFactorAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        var disabled = await users.SetTwoFactorEnabledAsync(user, false);
        if (!disabled.Succeeded) return IdentityErrors.ToError(disabled);

        var reset = await users.ResetAuthenticatorKeyAsync(user);
        return reset.Succeeded ? Result.Success() : IdentityErrors.ToError(reset);
    }

    /// <inheritdoc />
    /// <remarks>The old codes stop working.</remarks>
    public async Task<Result<IReadOnlyCollection<string>>> GenerateRecoveryCodesAsync(Guid userId,
                                                                                     CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;
        if (!user.TwoFactorEnabled) return AuthErrors.TwoFactorNotEnabled;

        return await NewRecoveryCodesAsync(user);
    }

    // ───────────────────────────── 4. Delete account ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>
    /// The row stays (orders and parcels point at the same Id), but nothing in it can identify the person or sign in:
    /// placeholder email, no phone, no password, no 2FA, no roles, deactivated and locked, every session ended.
    /// </remarks>
    public async Task<Result> AnonymiseAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await FindAsync(userId);
        if (user is null) return AuthErrors.UserNotFound;

        // 1. Roles
        var roles = await users.GetRolesAsync(user);
        if (roles.Count > 0)
        {
            var removed = await users.RemoveFromRolesAsync(user, roles);
            if (!removed.Succeeded) return IdentityErrors.ToError(removed);
        }

        // 2. Login details
        var placeholder = $"deleted-{user.Id:N}@{DeletedEmailDomain}";
        user.UserName = placeholder;
        user.Email = placeholder;
        user.EmailConfirmed = false;
        user.PhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.PasswordHash = null;
        user.TwoFactorEnabled = false;
        user.IsActive = false;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        // 3. Sessions (saved together with the user below: same database context)
        await RevokeAllAsync(user.Id);

        // UpdateAsync normalises the new email / user name, changes the security stamp and saves everything
        var updated = await users.UpdateAsync(user);
        return updated.Succeeded ? Result.Success() : IdentityErrors.ToError(updated);
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>Loads the login account (tracked).</summary>
    private Task<ApplicationUser?> FindAsync(Guid userId) => users.FindByIdAsync(userId.ToString());

    /// <summary>Makes a fresh set of recovery codes (the old ones stop working).</summary>
    private async Task<Result<IReadOnlyCollection<string>>> NewRecoveryCodesAsync(ApplicationUser user)
    {
        var codes = await users.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);
        return Result.Success<IReadOnlyCollection<string>>((codes ?? []).ToArray());
    }

    /// <summary>Revokes every working refresh token of a user (not saved).</summary>
    private async Task RevokeAllAsync(Guid userId)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        foreach (var token in await refreshTokens.GetActiveForUserAsync(userId, now))
            token.Revoke(now);
    }

    /// <summary>Revokes every working refresh token of a user and saves.</summary>
    private async Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct)
    {
        await RevokeAllAsync(userId);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>The key in groups of four, lower-case, for typing by hand: "abcd efgh ijkl …".</summary>
    private static string FormatKey(string key)
    {
        var result = new StringBuilder();
        for (var i = 0; i < key.Length; i += 4)
        {
            if (i > 0) result.Append(' ');
            result.Append(key.AsSpan(i, Math.Min(4, key.Length - i)));
        }
        return result.ToString().ToLowerInvariant();
    }

    /// <summary>The otpauth:// link the QR code holds (standard format every authenticator app reads).</summary>
    private string AuthenticatorUri(string account, string key)
    {
        var issuer = Uri.EscapeDataString(identityOptions.Value.AuthenticatorIssuer);
        return $"otpauth://totp/{issuer}:{Uri.EscapeDataString(account)}?secret={key}&issuer={issuer}&digits=6";
    }
}
