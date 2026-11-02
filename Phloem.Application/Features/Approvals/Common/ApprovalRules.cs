using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Approvals.Common;

/// <summary>
/// Which changes need a second person, and who that second person may be.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description><b>Terms and Privacy Policy</b> (publishing a new version): another owner (Admin). Help
///   stays one-step.</description></item>
///   <item><description><b>Blocking a shop</b>: applied at once; another owner, or a Country manager covering the
///   shop's country, confirms it.</description></item>
///   <item><description>Nobody decides their own change.</description></item>
/// </list>
/// <para>Prices, commission, the Admin role and job role permissions stay one-step for now (decided 2 Nov 2026);
/// they're logged in the audit log. Refunds and withdrawals join when payments are built.</para>
/// </remarks>
public static class ApprovalRules
{
    /// <summary>True when publishing this document needs another owner's approval.</summary>
    /// <param name="type">The document.</param>
    public static bool NeedsApproval(LegalDocumentType type) =>
        type is LegalDocumentType.Terms or LegalDocumentType.Privacy;

    /// <summary>True when this person may approve or reject this change.</summary>
    /// <param name="person">The signed-in person.</param>
    /// <param name="change">The change.</param>
    public static bool CanDecide(ICurrentUser person, PendingChange change)
    {
        if (person.UserId is not { } id || id == change.ProposedById) return false;
        if (person.IsInRole(Roles.Admin)) return true;

        return change.Kind == PendingChangeKind.ConfirmShopBlock
               && person.IsInRole(Roles.CountryManager)
               && change.Country is { } country
               && person.CoversCountry(country);
    }

    /// <summary>True when this person may SEE the change (decide it, proposed it, or is an owner).</summary>
    /// <param name="person">The signed-in person.</param>
    /// <param name="change">The change.</param>
    public static bool CanSee(ICurrentUser person, PendingChange change) =>
        person.UserId == change.ProposedById || person.IsInRole(Roles.Admin) || CanDecide(person, change)
        || (change.Country is { } c && person.CoversCountry(c));

    /// <summary>True when the proposer may take it back (Terms proposals; a block is undone by unblocking).</summary>
    /// <param name="person">The signed-in person.</param>
    /// <param name="change">The change.</param>
    public static bool CanWithdraw(ICurrentUser person, PendingChange change) =>
        person.UserId == change.ProposedById && change.Kind == PendingChangeKind.PublishLegalDocument;

    /// <summary>"Terms &amp; Conditions" / "Privacy Policy" / "Help".</summary>
    /// <param name="type">The document.</param>
    public static string DocumentName(LegalDocumentType type) => type switch
    {
        LegalDocumentType.Terms => "Terms & Conditions",
        LegalDocumentType.Privacy => "Privacy Policy",
        _ => type.ToString()
    };
}
