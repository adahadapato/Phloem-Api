using MediatR;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Queries;

/// <summary>Admin → Terms &amp; Help: how many people have accepted the current Terms. Never cached.</summary>
public sealed record GetTermsAcceptanceStatsQuery : IRequest<Result<TermsAcceptanceStatsDto>>;

/// <summary>Handles <see cref="GetTermsAcceptanceStatsQuery"/>.</summary>
public sealed class GetTermsAcceptanceStatsQueryHandler(
    ILegalDocumentRepository documents,
    ITermsAcceptanceRepository acceptances,
    IUserProfileRepository profiles)
    : IRequestHandler<GetTermsAcceptanceStatsQuery, Result<TermsAcceptanceStatsDto>>
{
    public async Task<Result<TermsAcceptanceStatsDto>> Handle(GetTermsAcceptanceStatsQuery query, CancellationToken ct)
    {
        var current = await documents.GetLatestVersionAsync(LegalDocumentType.Terms, ct);
        var accepted = current == 0 ? 0 : await acceptances.CountPeopleAtLeastAsync(LegalDocumentType.Terms, current, ct);
        var people = (await profiles.GetPagedAsync(null, new PageRequest { Page = 1, PageSize = 1 }, ct)).TotalCount;
        return new TermsAcceptanceStatsDto(current, accepted, people);
    }
}
