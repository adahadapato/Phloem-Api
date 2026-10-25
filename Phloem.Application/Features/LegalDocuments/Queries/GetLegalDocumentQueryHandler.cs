using MediatR;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Queries;

/// <summary>Handles <see cref="GetLegalDocumentQuery"/>.</summary>
public sealed class GetLegalDocumentQueryHandler(ILegalDocumentRepository documents)
    : IRequestHandler<GetLegalDocumentQuery, Result<LegalDocumentDto>>
{
    /// <summary>The highest version of the type, or "not available yet".</summary>
    public async Task<Result<LegalDocumentDto>> Handle(GetLegalDocumentQuery query, CancellationToken ct)
    {
        var current = await documents.GetCurrentAsync(query.Type, ct);
        return current is null ? LegalDocumentErrors.NotFound : LegalDocumentDto.From(current);
    }
}
