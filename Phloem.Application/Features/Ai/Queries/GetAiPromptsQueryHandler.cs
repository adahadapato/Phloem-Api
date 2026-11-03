using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Handles <see cref="GetAiPromptsQuery"/>.</summary>
public sealed class GetAiPromptsQueryHandler(IAiPromptVersionRepository versions, IUserProfileRepository profiles)
    : IRequestHandler<GetAiPromptsQuery, Result<IReadOnlyList<AiPromptSummaryDto>>>
{
    /// <summary>The prompts from code, each with its newest saved version (if any).</summary>
    public async Task<Result<IReadOnlyList<AiPromptSummaryDto>>> Handle(GetAiPromptsQuery query, CancellationToken ct)
    {
        var latest = (await versions.GetLatestForAllAsync(ct)).ToDictionary(v => v.Key);
        var names = await profiles.GetFullNamesAsync(latest.Values.Select(v => v.SavedBy).Distinct().ToList(), ct);

        return AiPrompts.All.Select(p =>
        {
            var v = latest.GetValueOrDefault(p.Key);
            return new AiPromptSummaryDto(p.Key, p.Name, p.Description, v?.Version ?? 0, v?.CreatedAt,
                                          v is null ? null : names.GetValueOrDefault(v.SavedBy, "Unknown user"));
        }).ToList();
    }
}
