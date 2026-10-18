namespace Phloem.Domain.Shared.Errors;

/// <summary>Login / account failures returned by IAuthService, IAccountService and IUserRoleService.</summary>
public static class AuthErrors
{
    // ---------- Sign-in ----------

    // Deliberately vague: never reveal whether the email exists
    public static readonly Error InvalidCredentials = Error.Validation(
        "Auth.InvalidCredentials", "The email or password is incorrect.");

    public static readonly Error EmailTaken = Error.Conflict(
        "Auth.EmailTaken", "An account with this email already exists.");

    public static readonly Error EmailNotConfirmed = Error.Forbidden(
        "Auth.EmailNotConfirmed", "Please confirm your email address first. Check your inbox.");

    public static readonly Error LockedOut = Error.Forbidden(
        "Auth.LockedOut", "Too many failed attempts. Please try again in a few minutes.");

    /// <summary>An admin has switched the account off (not the same as a temporary lockout).</summary>
    public static readonly Error AccountDeactivated = Error.Forbidden(
        "Auth.AccountDeactivated", "This account has been deactivated. Please contact support.");

    public static readonly Error InvalidToken = Error.Validation(
        "Auth.InvalidToken", "This link or session has expired. Please sign in again.");

    // ---------- Account ----------

    public static readonly Error WrongPassword = Error.Validation(
        "Auth.WrongPassword", "Your current password isn't right.");

    public static readonly Error UserNotFound = Error.NotFound(
        "Auth.UserNotFound", "That account wasn't found.");

    // ---------- Two-factor ----------

    public static readonly Error InvalidTwoFactorCode = Error.Validation(
        "Auth.InvalidTwoFactorCode", "That code isn't right. Please try again.");

    /// <summary>Admins must keep two-factor sign-in switched on.</summary>
    public static readonly Error TwoFactorRequired = Error.Forbidden(
        "Auth.TwoFactorRequired", "Two-factor sign-in is required for admin accounts and can't be switched off.");

    /// <summary>Recovery codes only exist while two-factor sign-in is on.</summary>
    public static readonly Error TwoFactorNotEnabled = Error.Validation(
        "Auth.TwoFactorNotEnabled", "Turn on two-factor sign-in first.");

    // ---------- Roles ----------

    /// <summary>A role name that isn't one of <c>Roles.All</c>.</summary>
    public static readonly Error UnknownRole = Error.Validation(
        "Auth.UnknownRole", "That role doesn't exist.");

    /// <summary>Removing the Admin role from the only admin would leave nobody able to manage Phloem.</summary>
    public static readonly Error LastAdmin = Error.Conflict(
        "Auth.LastAdmin", "This is the only admin account, so the Admin role can't be removed.");

    // ---------- Access (written by Presentation when a request is refused) ----------

    /// <summary>No access token sent.</summary>
    public static readonly Error SignInRequired = Error.Unauthorized(
        "Auth.SignInRequired", "Please sign in to do this.");

    /// <summary>The access token has expired: refresh it, or sign in again.</summary>
    public static readonly Error SessionExpired = Error.Unauthorized(
        "Auth.SessionExpired", "Your session has expired. Please sign in again.");

    /// <summary>The access token is damaged, forged or for another app.</summary>
    public static readonly Error InvalidAccessToken = Error.Unauthorized(
        "Auth.InvalidAccessToken", "Your sign-in isn't valid. Please sign in again.");

    /// <summary>Signed in, but this needs a role the user doesn't have.</summary>
    public static readonly Error NotAllowed = Error.Forbidden(
        "Auth.NotAllowed", "You don't have permission to do this.");
}