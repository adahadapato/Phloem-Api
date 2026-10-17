using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Phloem.Domain.ValueObjects;

namespace Phloem.Persistence.Converters;

/// <summary>
/// Stores a <see cref="PhoneNumber"/> in its normalised form (e.g. "+233245550192": an optional "+" then digits);
/// reading it back goes through <see cref="PhoneNumber.Create"/>.
/// </summary>
/// <remarks>
/// <para>Use with <c>.HasMaxLength(PhoneNumberConverter.MaxLength)</c>.</para>
/// <para>Nulls never reach a converter, so it also works for optional phones.</para>
/// </remarks>
/// <example>
/// <code>
/// builder.Property(v => v.BusinessPhone)
///        .HasConversion(new PhoneNumberConverter())
///        .HasMaxLength(PhoneNumberConverter.MaxLength);
/// </code>
/// </example>
public sealed class PhoneNumberConverter : ValueConverter<PhoneNumber, string>
{
    /// <summary>Column size: an optional "+" plus at most <see cref="PhoneNumber.MaxDigits"/> digits.</summary>
    public const int MaxLength = PhoneNumber.MaxDigits + 1;

    /// <summary>Creates the converter.</summary>
    public PhoneNumberConverter()
        : base(phone => phone.Value, value => PhoneNumber.Create(value)) { }
}