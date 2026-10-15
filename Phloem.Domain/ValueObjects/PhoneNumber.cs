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

    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static PhoneNumber Create(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        var trimmed = phoneNumber.Trim();

        // Allowed when typing: digits, spaces, dashes, brackets, and "+" only at the start
        foreach (var (c, i) in trimmed.Select((c, i) => (c, i)))
        {
            var ok = char.IsAsciiDigit(c) || c is ' ' or '-' or '(' or ')' || (c == '+' && i == 0);
            if (!ok)
                throw new ArgumentException("That phone number doesn't look right.", nameof(phoneNumber));
        }

        var digits = new string(trimmed.Where(char.IsAsciiDigit).ToArray());
        if (digits.Length is < MinDigits or > MaxDigits)
            throw new ArgumentException("That phone number doesn't look right.", nameof(phoneNumber));

        return new PhoneNumber(trimmed.StartsWith('+') ? "+" + digits : digits);
    }

    /// <summary>For optional numbers: blank → null, otherwise a valid PhoneNumber.</summary>
    public static PhoneNumber? CreateOptional(string? phoneNumber) =>
        string.IsNullOrWhiteSpace(phoneNumber) ? null : Create(phoneNumber);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}