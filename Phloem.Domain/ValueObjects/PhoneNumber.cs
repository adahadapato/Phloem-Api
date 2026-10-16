namespace Phloem.Domain.ValueObjects;

/// <summary>
/// A phone number, stored in one consistent shape: an optional leading "+" then digits only.
/// "+233 24-555 (0192)" → "+233245550192". So the same number typed two ways is equal.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    /// <summary>The international standard (E.164) allows at most 15 digits.</summary>
    public const int MaxDigits = 15;
    public const int MinDigits = 7;

    /// <summary>Shared with the FluentValidation rules so the API and Domain say the same thing.</summary>
    public const string InvalidMessage = "That phone number doesn't look right.";

    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    /// <summary>For validators: true if Create would accept this text.</summary>
    public static bool IsValid(string? phoneNumber) => Normalise(phoneNumber) is not null;

    public static PhoneNumber Create(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var normalised = Normalise(phoneNumber)
            ?? throw new ArgumentException(InvalidMessage, nameof(phoneNumber));

        return new PhoneNumber(normalised);
    }

    /// <summary>For optional numbers: blank → null, otherwise a valid PhoneNumber.</summary>
    public static PhoneNumber? CreateOptional(string? phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? null : Create(phoneNumber);

    /// <summary>
    /// The one place the rules live. Returns "+digits" / "digits", or null if not a valid number.
    /// </summary>
    private static string? Normalise(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var trimmed = phoneNumber.Trim();

        // Allowed when typing: digits, spaces, dashes, brackets, and "+" only at the start
        for (var i = 0; i < trimmed.Length; i++)
        {
            var c = trimmed[i];
            var ok = char.IsAsciiDigit(c) || c is ' ' or '-' or '(' or ')' || (c == '+' && i == 0);
            if (!ok) return null;
        }

        var digits = new string(trimmed.Where(char.IsAsciiDigit).ToArray());
        if (digits.Length is < MinDigits or > MaxDigits)
            return null;

        return trimmed.StartsWith('+') ? "+" + digits : digits;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}