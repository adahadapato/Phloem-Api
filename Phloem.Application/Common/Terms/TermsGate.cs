using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Common.Terms;

/// <summary>Where someone stands with the Terms and the Privacy Policy (shown on their profile).</summary>
/// <param name="CurrentTermsVersion">The latest published Terms (0 = none published yet).</param>
/// <param name="AcceptedTermsVersion">The highest version they accepted, or null.</param>
/// <param name="CurrentPrivacyVersion">The latest published Privacy Policy (0 = none).</param>
/// <param name="SeenPrivacyVersion">The highest Privacy version they were shown, or null.</param>
public sealed record TermsStatus(int CurrentTermsVersion, int? AcceptedTermsVersion,
                                 int CurrentPrivacyVersion, int? SeenPrivacyVersion)
{
    /// <summary>True when they must accept the latest Terms before ordering, sending parcels or selling.</summary>
    public bool NeedsToAcceptTerms => CurrentTermsVersion > 0 && (AcceptedTermsVersion ?? 0) < CurrentTermsVersion;

    /// <summary>True when a newer Privacy Policy should be shown to them once ("Got it").</summary>
    public bool NeedsToSeePrivacy => CurrentPrivacyVersion > 0 && (SeenPrivacyVersion ?? 0) < CurrentPrivacyVersion;
}

/// <summary>
/// The one place that knows the Terms rules (used by handlers, never by endpoints):
/// <list type="bullet">
///   <item><description><see cref="EnsureAcceptedAsync"/>: ordering, adding dishes… need the latest Terms.</description></item>
///   <item><description><see cref="RecordAsync"/>: adds an acceptance of the CURRENT version (the caller saves).</description></item>
/// </list>
/// </summary>
/// <remarks>Registered in <see cref="DependencyInjection.AddApplication"/>. A version is the latest published one,
/// drafts included (a draft is still what people see and agree to).</remarks>
public sealed class TermsGate(ILegalDocumentRepository documents, ITermsAcceptanceRepository acceptances)
{
    /// <summary>Their status with both documents.</summary>
    public async Task<TermsStatus> GetStatusAsync(Guid userId, CancellationToken ct)
    {
        var terms = await documents.GetLatestVersionAsync(LegalDocumentType.Terms, ct);
        var accepted = await acceptances.GetLatestVersionAsync(userId, LegalDocumentType.Terms, ct);
        var privacy = await documents.GetLatestVersionAsync(LegalDocumentType.Privacy, ct);
        var seen = await acceptances.GetLatestVersionAsync(userId, LegalDocumentType.Privacy, ct);
        return new TermsStatus(terms, accepted, privacy, seen);
    }

    /// <summary>Success when they've accepted the latest Terms (or none are published); otherwise Terms.NotAccepted.</summary>
    public async Task<Result> EnsureAcceptedAsync(Guid userId, CancellationToken ct)
    {
        var current = await documents.GetLatestVersionAsync(LegalDocumentType.Terms, ct);
        if (current == 0) return Result.Success();

        var accepted = await acceptances.GetLatestVersionAsync(userId, LegalDocumentType.Terms, ct);
        return (accepted ?? 0) >= current ? Result.Success() : TermsErrors.NotAccepted;
    }

    /// <summary>
    /// Adds an acceptance of the document's current version (nothing when none is published). The caller saves it
    /// together with whatever it belongs to (the account, the order, the parcel…).
    /// </summary>
    /// <returns>The version recorded, or 0 when there was nothing to accept.</returns>
    public async Task<int> RecordAsync(Guid userId, LegalDocumentType documentType, AcceptanceContext context,
                                  string? reference, CancellationToken ct)
    {
        var current = await documents.GetLatestVersionAsync(documentType, ct);
        if (current == 0) return 0;
        acceptances.Add(TermsAcceptance.Create(userId, documentType, current, context, reference));
        return current;
    }
}
