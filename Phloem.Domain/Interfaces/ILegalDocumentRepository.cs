using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>Loads and saves <see cref="LegalDocument"/> versions. Implemented in Persistence.</summary>
public interface ILegalDocumentRepository : IRepository<LegalDocument>
{
    /// <summary>The current text of a type: its highest version; null if none has been published.</summary>
    /// <param name="type">Terms or Privacy.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<LegalDocument?> GetCurrentAsync(LegalDocumentType type, CancellationToken cancellationToken = default);

    /// <summary>The highest version number of a type; 0 if none.</summary>
    /// <param name="type">Terms or Privacy.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<int> GetLatestVersionAsync(LegalDocumentType type, CancellationToken cancellationToken = default);
}
