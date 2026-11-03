using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Handles <see cref="GetAiStatusQuery"/>.</summary>
public sealed class GetAiStatusQueryHandler(IAiSettingsProvider settings, IAiCallLogRepository calls, TimeProvider time)
    : IRequestHandler<GetAiStatusQuery, Result<AiStatusDto>>
{
    /// <summary>Settings plus this month's totals from the AI log.</summary>
    public async Task<Result<AiStatusDto>> Handle(GetAiStatusQuery query, CancellationToken ct)
    {
        var s = settings.Get();
        var now = time.GetUtcNow().UtcDateTime;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var usage = await calls.GetUsageSinceAsync(monthStart, ct);

        return new AiStatusDto(s.Provider, s.Model, s.IsEnabled, s.NotReadyReason, usage.Calls, usage.FailedCalls,
                               usage.InputTokens, usage.OutputTokens, Math.Round(usage.CostUsd, 4), s.MonthlyCapUsd,
                               usage.CostUsd >= s.MonthlyCapUsd, s.LogRetentionMonths);
    }
}
