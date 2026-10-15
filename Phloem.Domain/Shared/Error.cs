

using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Shared;

/// <summary>
/// Represents a concrete domain error.
/// </summary>
public sealed class Error : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public Error(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public static Error NullValue => new Error("Error.Null", "The value cannot be null.");

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the empty error instance.
    /// </summary>
    internal static Error None { get; } = new Error(string.Empty, string.Empty);

    /// <summary>
    /// Implicitly converts the error to its code.
    /// </summary>
    public static implicit operator string(Error error) => error?.Code ?? string.Empty;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Message;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Code}: {Message}";
}
