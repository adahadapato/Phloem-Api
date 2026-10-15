using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.ValueObjects;

/// <summary>
/// Represents a valid absolute URL.
/// </summary>
public class Url : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; private set; } = string.Empty;

    private Url() { }

    private Url(string value)
    {
        Value = value.Trim();
    }

    /// <summary>
    /// Creates a new URL with validation.
    /// </summary>
    public static Result<Url> Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result<Url>.Failure(
                new Error("Url.Empty", "URL is required."));

        if (url.Length > MaxLength)
            return Result<Url>.Failure(
                new Error("Url.TooLong", $"URL exceeds {MaxLength} characters."));

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            return Result<Url>.Failure(
                new Error("Url.Invalid", "URL must be a valid absolute URI."));

        return Result<Url>.Success(new Url(url));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
