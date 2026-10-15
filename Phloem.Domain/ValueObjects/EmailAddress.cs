using Phloem.Domain.Shared;
using System.Text.RegularExpressions;

namespace Phloem.Domain.ValueObjects;

/// <summary>
/// Represents a valid email address.
/// </summary>
public class EmailAddress : ValueObject
{
    public const int MaxLength = 256;

    public string Value { get; private set; } = string.Empty;

    private EmailAddress() { }

    private EmailAddress(string value)
    {
        Value = value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Creates a new email address with validation.
    /// </summary>
    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<EmailAddress>.Failure(
                new Error("EmailAddress.Empty", "Email is required."));

        if (email.Length > MaxLength)
            return Result<EmailAddress>.Failure(
                new Error("EmailAddress.TooLong", $"Email exceeds {MaxLength} characters."));

        if (!IsValid(email))
            return Result<EmailAddress>.Failure(
                new Error("EmailAddress.InvalidFormat", "Email format is invalid."));

        return Result<EmailAddress>.Success(new EmailAddress(email));
    }

    private static bool IsValid(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
