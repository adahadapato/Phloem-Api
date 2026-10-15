using System.Net.Mail;

namespace Phloem.Domain.ValueObjects;

/// <summary>
/// A valid, normalised email address (trimmed, lower-case).
/// If you hold an EmailAddress, it is valid: it can only be made through Create.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    public const int MaxLength = 254;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static EmailAddress Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        var normalised = email.Trim().ToLowerInvariant();

        if (normalised.Length > MaxLength)
            throw new ArgumentException($"Email must be {MaxLength} characters or fewer.", nameof(email));

        // MailAddress does the hard part of the format check. We also insist on a dot in the
        // domain ("ama@gmail" is technically valid but never right for Phloem customers),
        // and that it didn't need cleaning up ("Ama <ama@x.com>" is rejected).
        if (!MailAddress.TryCreate(normalised, out var parsed) ||
            parsed.Address != normalised ||
            !parsed.Host.Contains('.') ||
            parsed.Host.StartsWith('.') || parsed.Host.EndsWith('.'))
            throw new ArgumentException("That email address doesn't look right.", nameof(email));

        return new EmailAddress(normalised);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}