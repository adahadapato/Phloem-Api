namespace Phloem.Domain.Models;

/// <summary>
/// What the Contact us page shows, from settings (<c>Contact</c> + <c>Email:ReplyTo:Support</c>), through
/// <c>IContactDetailsProvider</c>. Every value except the email may be null: the page then leaves it out.
/// </summary>
/// <param name="Email">The support inbox, e.g. "support@phloem.app".</param>
/// <param name="Phone">e.g. "+44 20 7946 0000".</param>
/// <param name="WhatsApp">e.g. "+44 7700 900000".</param>
/// <param name="Address">One line, e.g. "Phloem, 1 Example Street, London EC1A 1AA, United Kingdom".</param>
/// <param name="OpeningHours">e.g. "Monday to Saturday, 9am to 6pm (UK time)".</param>
/// <param name="ReplyTime">e.g. "within 24 hours".</param>
public sealed record ContactDetails(
    string Email,
    string? Phone,
    string? WhatsApp,
    string? Address,
    string? OpeningHours,
    string? ReplyTime);
