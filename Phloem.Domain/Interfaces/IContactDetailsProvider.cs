using Phloem.Domain.Models;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// The contact details shown on Contact us and used in emails. Implemented in Infrastructure from settings
/// (<c>Contact</c> and <c>Email:ReplyTo:Support</c>), so they change without a code change.
/// </summary>
public interface IContactDetailsProvider
{
    /// <summary>The current details (empty settings come back as null).</summary>
    ContactDetails Get();
}
