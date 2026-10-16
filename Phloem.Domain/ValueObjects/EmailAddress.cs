using System.Net.Mail;

namespace Phloem.Domain.ValueObjects;

/// <summary>
/// A valid, normalised email address (trimmed, lower-case).
/// If you hold an EmailAddress, it is valid: it can only be made through Create.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    public const int MaxLength = 254;

    /// <summary>Shared with the FluentValidation rules so the API and Domain say the same thing.</summary>
    public const string InvalidMessage = "That email address doesn't look right.";

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    /// <summary>For validators: true if Create would accept this text.</summary>
    public static bool IsValid(string? email) => Normalise(email) is not null;

    public static EmailAddress Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (email.Trim().Length > MaxLength)
            throw new ArgumentException($"Email must be {MaxLength} characters or fewer.", nameof(email));

        var normalised = Normalise(email)
            ?? throw new ArgumentException(InvalidMessage, nameof(email));

        return new EmailAddress(normalised);
    }

    /// <summary>
    /// The one place the rules live. Returns the trimmed, lower-case email, or null if not valid.
    /// </summary>
    private static string? Normalise(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalised = email.Trim().ToLowerInvariant();

        if (normalised.Length > MaxLength)
            return null;

        // MailAddress does the hard part of the format check. We also insist on a dot in the
        // domain ("ama@gmail" is technically valid but never right for Phloem customers),
        // and that it didn't need cleaning up ("Ama <ama@x.com>" is rejected).
        if (!MailAddress.TryCreate(normalised, out var parsed) ||
            parsed.Address != normalised ||
            !parsed.Host.Contains('.') ||
            parsed.Host.StartsWith('.') || parsed.Host.EndsWith('.'))
            return null;

        return normalised;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}