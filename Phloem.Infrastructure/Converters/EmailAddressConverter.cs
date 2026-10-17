using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Phloem.Domain.ValueObjects;
using System.Net.Mail;

namespace Phloem.Configuration.Converters;

/// <summary>Stores an <see cref="EmailAddress"/> as its text; reading it back goes through <see cref="EmailAddress.Create"/>.</summary>
/// <remarks>Use with <c>.HasMaxLength(EmailAddress.MaxLength)</c>. Nulls never reach a converter, so it also works for optional emails.</remarks>
public sealed class EmailAddressConverter : ValueConverter<EmailAddress, string>
{
    /// <summary>Creates the converter.</summary>
    public EmailAddressConverter()
        : base(email => email.Value, value => EmailAddress.Create(value)) { }
}