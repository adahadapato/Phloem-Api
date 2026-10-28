using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>Terms and Privacy acceptances (added, never changed). Implemented in Persistence.</summary>
public interface ITermsAcceptanceRepository : IRepository<TermsAcceptance>
{
    /// <summary>The highest version of a document someone has accepted (or been shown), or null if none.</summary>
    /// <param name="userId">Who.</param>
    /// <param name="documentType">Terms or Privacy.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<int?> GetLatestVersionAsync(Guid userId, LegalDocumentType documentType,
                                     CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin → Users: for a page of people, the highest Terms version each accepted and when they first accepted
    /// that version, in ONE query. People who never accepted are missing.
    /// </summary>
    /// <param name="userIds">The people on the page.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyDictionary<Guid, (int Version, DateTime AcceptedAt)>> GetLatestTermsAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);

    /// <summary>How many different people have accepted (at least) this version of a document.</summary>
    /// <param name="documentType">Terms or Privacy.</param>
    /// <param name="version">The version.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<int> CountPeopleAtLeastAsync(LegalDocumentType documentType, int version,
                                      CancellationToken cancellationToken = default);
}
