using System.Text.RegularExpressions;

namespace Phloem.Domain.Rules;

/// <summary>
/// Spots text that may take a deal outside Phloem: phone numbers, email addresses, bank or mobile-money details, or
/// "pay me directly". Used to show the writer a gentle warning before sending a chat message (nothing is blocked)
/// and to mark the message for staff. The website runs the same check while typing.
/// </summary>
public static partial class OffPlatformHints
{
    /// <summary>The warning shown before sending.</summary>
    public const string Warning =
        "It looks like this message has contact or payment details. Payments made outside Phloem aren't protected, " +
        "so please keep ordering and paying on Phloem.";

    /// <summary>True when the text looks like it shares contact or payment details.</summary>
    /// <param name="text">The message.</param>
    public static bool Found(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var plain = PhloemNumber().Replace(text, " ");   // "PH-261016-482193" is an order number, not a phone
        return Phone().IsMatch(plain) || Email().IsMatch(plain) || Words().IsMatch(plain);
    }

    // Phloem's own order and tracking numbers
    [GeneratedRegex(@"\bPHS?-\d{6}-\d{6}\b", RegexOptions.IgnoreCase)]
    private static partial Regex PhloemNumber();

    // 8+ digits, allowing spaces, dashes, dots, brackets and a leading +, e.g. "024 123 4567", "+44 7700 900000"
    [GeneratedRegex(@"\+?\d[\d\s\-().]{6,}\d")]
    private static partial Regex Phone();

    [GeneratedRegex(@"[\w.+\-]+@[\w\-]+\.[\w.]+")]
    private static partial Regex Email();

    [GeneratedRegex(@"\b(whats\s?app|telegram|bank\s+transfer|account\s+(number|no)|acct\s*no|sort\s+code|iban|momo|mobile\s+money|cash\s?app|pay\s+(me\s+)?(directly|outside|cash)|send\s+(the\s+)?money\s+to|my\s+number)\b",
                    RegexOptions.IgnoreCase)]
    private static partial Regex Words();
}
