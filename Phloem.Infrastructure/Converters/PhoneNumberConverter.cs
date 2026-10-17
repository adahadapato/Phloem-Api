using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Phloem.Domain.ValueObjects;

namespace Phloem.Configuration.Converters;

/// <summary>Stores a <see cref="PhoneNumber"/> in its normalised form ("+233245550192").</summary>
/// <remarks>Nulls never reach a converter, so it also works for optional phones.</remarks>
public sealed class PhoneNumberConverter : ValueConverter<PhoneNumber, string>
{
    /// <summary>Column size: an optional "+" plus at most <see cref="PhoneNumber.MaxDigits"/> digits.</summary>
    public const int MaxLength = PhoneNumber.MaxDigits + 1;

    /// <summary>Creates the converter.</summary>
    public PhoneNumberConverter()
        : base(phone => phone.Value, value => PhoneNumber.Create(value)) { }
}