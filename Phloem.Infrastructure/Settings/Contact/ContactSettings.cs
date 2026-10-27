using System.ComponentModel.DataAnnotations;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Contact;

/// <summary>
/// The <c>"Contact"</c> section of appsettings.json: the details on the Contact us page (keys in
/// <see cref="AppSettings.Contact"/>). Every value is optional; an empty one isn't shown. The email shown is
/// <c>Email:ReplyTo:Support</c>, so it's set in one place.
/// </summary>
/// <remarks>Not secret: these are public details, so they can live in appsettings.json.</remarks>
public sealed class ContactSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = AppSettings.Contact.Section;

    /// <summary>Phone number, e.g. "+44 20 7946 0000".</summary>
    [MaxLength(30)]
    public string? Phone { get; set; }

    /// <summary>WhatsApp number in international format, e.g. "+44 7700 900000".</summary>
    [MaxLength(30)]
    public string? WhatsApp { get; set; }

    /// <summary>Postal address on one line.</summary>
    [MaxLength(200)]
    public string? Address { get; set; }

    /// <summary>When the team answers, e.g. "Monday to Saturday, 9am to 6pm (UK time)".</summary>
    [MaxLength(100)]
    public string? OpeningHours { get; set; }

    /// <summary>How soon people get an answer, e.g. "within 24 hours".</summary>
    [MaxLength(60)]
    public string? ReplyTime { get; set; }
}
