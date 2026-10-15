namespace Phloem.Domain.Shared.Errors;

/// <summary>Login / account failures returned by IAuthService and IAccountService.</summary>
public static class AuthErrors
{
    // Deliberately vague: never reveal whether the email exists
    public static readonly Error InvalidCredentials = Error.Validation(
        "Auth.InvalidCredentials", "The email or password is incorrect.");

    public static readonly Error EmailTaken = Error.Conflict(
        "Auth.EmailTaken", "An account with this email already exists.");

    public static readonly Error EmailNotConfirmed = Error.Forbidden(
        "Auth.EmailNotConfirmed", "Please confirm your email address first. Check your inbox.");

    public static readonly Error LockedOut = Error.Forbidden(
        "Auth.LockedOut", "Too many failed attempts. Please try again in a few minutes.");

    public static readonly Error InvalidTwoFactorCode = Error.Validation(
        "Auth.InvalidTwoFactorCode", "That code isn't right. Please try again.");

    public static readonly Error InvalidToken = Error.Validation(
        "Auth.InvalidToken", "This link or session has expired. Please sign in again.");

    public static readonly Error WrongPassword = Error.Validation(
        "Auth.WrongPassword", "Your current password isn't right.");

    public static readonly Error UserNotFound = Error.NotFound(
        "Auth.UserNotFound", "That account wasn't found.");
}
