using MediatR;
using Phloem.Application.Features.Ai.Common;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Handles <see cref="GetAiCallsQuery"/>.</summary>
public sealed class GetAiCallsQueryHandler(IAiCallLogRepository calls)
    : IRequestHandler<GetAiCallsQuery, Result<PagedResult<AiCallSummaryDto>>>
{
    /// <summary>One page of the log.</summary>
    public async Task<Result<PagedResult<AiCallSummaryDto>>> Handle(GetAiCallsQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await calls.GetPagedAsync(query.PromptKey?.Trim(), query.Outcome, page, ct);
        return new PagedResult<AiCallSummaryDto>(paged.Items.Select(c => c.ToSummary()).ToList(),
                                                 paged.TotalCount, paged.Page, paged.PageSize);
    }
}
