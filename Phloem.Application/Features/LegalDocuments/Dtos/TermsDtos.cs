using Phloem.Application.Common.Terms;

namespace Phloem.Application.Features.LegalDocuments.Dtos;

/// <summary>
/// On <c>GET /api/me</c>: where I stand with the Terms and the Privacy Policy. The website and app show
/// "We've updated our Terms" when <see cref="NeedsToAcceptTerms"/>, and a one-time Privacy notice when
/// <see cref="NeedsToSeePrivacy"/>.
/// </summary>
/// <param name="CurrentTermsVersion">The latest published Terms (0 = none yet).</param>
/// <param name="AcceptedTermsVersion">The highest version I accepted, or null.</param>
/// <param name="NeedsToAcceptTerms">True: ordering, sending parcels and selling wait until I accept.</param>
/// <param name="CurrentPrivacyVersion">The latest published Privacy Policy (0 = none yet).</param>
/// <param name="NeedsToSeePrivacy">True: show me the "updated Privacy Policy" notice once.</param>
public sealed record TermsStatusDto(
    int CurrentTermsVersion,
    int? AcceptedTermsVersion,
    bool NeedsToAcceptTerms,
    int CurrentPrivacyVersion,
    bool NeedsToSeePrivacy)
{
    public static TermsStatusDto From(TermsStatus s) =>
        new(s.CurrentTermsVersion, s.AcceptedTermsVersion, s.NeedsToAcceptTerms, s.CurrentPrivacyVersion,
            s.NeedsToSeePrivacy);
}

/// <summary><c>GET /api/admin/legal/acceptance</c>: "84 of 120 people have accepted version 3".</summary>
/// <param name="CurrentTermsVersion">The latest published Terms (0 = none yet).</param>
/// <param name="AcceptedCount">People who accepted that version (or a later one).</param>
/// <param name="PeopleCount">Everyone with an account.</param>
public sealed record TermsAcceptanceStatsDto(int CurrentTermsVersion, int AcceptedCount, int PeopleCount);
