using Phloem.Domain.Shared;
using System.Text.RegularExpressions;

namespace Phloem.Domain.ValueObjects;

/// <summary>
/// Represents a valid international phone number (e.g., +44 7123 456789).
/// </summary>
public class PhoneNumber : ValueObject
{
    public const int MaxLength = 20;

    public string Value { get; private set; } = string.Empty;

    private PhoneNumber() { }

    private PhoneNumber(string value)
    {
        Value = value.Trim();
    }

    /// <summary>
    /// Creates a new phone number with validation.
    /// </summary>
    public static Result<PhoneNumber> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Result<PhoneNumber>.Failure(
                new Error("PhoneNumber.Empty", "Phone number is required."));

        if (phone.Length > MaxLength)
            return Result<PhoneNumber>.Failure(
                new Error("PhoneNumber.TooLong", $"Phone number exceeds {MaxLength} characters."));

        if (!IsValid(phone))
            return Result<PhoneNumber>.Failure(
                new Error("PhoneNumber.Invalid", "Phone number must start with '+' and contain only digits and spaces."));

        return Result<PhoneNumber>.Success(new PhoneNumber(phone));
    }

    private static bool IsValid(string input)
    {
        var pattern = @"^\+\d{1,4}[\s\d]{6,15}$";
        return Regex.IsMatch(input, pattern);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}