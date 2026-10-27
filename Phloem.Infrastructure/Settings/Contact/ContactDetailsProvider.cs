using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Infrastructure.Settings.Email;

namespace Phloem.Infrastructure.Settings.Contact;

/// <summary>Implements <see cref="IContactDetailsProvider"/> from <see cref="ContactSettings"/> and the support inbox.</summary>
internal sealed class ContactDetailsProvider(IOptionsMonitor<ContactSettings> contact, IOptions<EmailSettings> email)
    : IContactDetailsProvider
{
    /// <inheritdoc />
    /// <remarks>Reads the settings each time (IOptionsMonitor), so a changed appsettings.json is picked up.</remarks>
    public ContactDetails Get()
    {
        var c = contact.CurrentValue;
        return new ContactDetails(
            email.Value.ReplyTo.Support,
            Clean(c.Phone),
            Clean(c.WhatsApp),
            Clean(c.Address),
            Clean(c.OpeningHours),
            Clean(c.ReplyTime));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
