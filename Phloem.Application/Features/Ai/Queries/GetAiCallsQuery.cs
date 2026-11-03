using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Admin → AI → Calls: the AI log, newest first, without text. Not cached.</summary>
/// <param name="PromptKey">Only this job; null = all.</param>
/// <param name="Outcome">Only calls that ended like this; null = all.</param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Calls per page.</param>
public sealed record GetAiCallsQuery(string? PromptKey, AiCallOutcome? Outcome, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<AiCallSummaryDto>>>;
