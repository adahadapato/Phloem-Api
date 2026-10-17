using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Phloem.Domain.ValueObjects;

namespace Phloem.Persistence.Converters;

/// <summary>
/// Stores an <see cref="EmailAddress"/> as its text (already normalised to lower case by the value object);
/// reading it back goes through <see cref="EmailAddress.Create"/>, so a stored value is always a valid email.
/// </summary>
/// <remarks>
/// <para>Use with <c>.HasMaxLength(EmailAddress.MaxLength)</c>.</para>
/// <para>Nulls never reach a converter, so it also works for optional emails.</para>
/// </remarks>
/// <example>
/// <code>
/// builder.Property(v => v.BusinessEmail)
///        .HasConversion(new EmailAddressConverter())
///        .HasMaxLength(EmailAddress.MaxLength);
/// </code>
/// </example>
public sealed class EmailAddressConverter : ValueConverter<EmailAddress, string>
{
    /// <summary>Creates the converter.</summary>
    public EmailAddressConverter()
        : base(email => email.Value, value => EmailAddress.Create(value)) { }
}