namespace Phloem.Domain.Constants;

/// <summary>
/// Small input checks shared by the entities (trim, required, max length, email, phone).
/// Internal: only the Domain uses it.
/// </summary>
internal static class Rules
{
    public static string Name(string? value, string what, int maxLength = 120)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException($"{what} is required.", what);
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{what} must be {maxLength} characters or fewer.", what);
        return trimmed;
    }

    public static string? Optional(string? value, string what, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{what} must be {maxLength} characters or fewer.", what);
        return trimmed;
    }

    public static string Email(string? value)
    {
        var email = Name(value, "Email", 254).ToLowerInvariant();
        var at = email.IndexOf('@');
        if (at < 1 || email.LastIndexOf('.') < at + 2 || email.EndsWith('.'))
            throw new ArgumentException("That email address doesn't look right.", "Email");
        return email;
    }

    public static string? Phone(string? value)
    {
        var phone = Optional(value, "Phone number", 30);
        if (phone is null) return null;
        if (phone.Count(char.IsDigit) < 7 ||
            phone.Any(c => !(char.IsDigit(c) || c is '+' or ' ' or '-' or '(' or ')')))
            throw new ArgumentException("That phone number doesn't look right.", "PhoneNumber");
        return phone;
    }
}
