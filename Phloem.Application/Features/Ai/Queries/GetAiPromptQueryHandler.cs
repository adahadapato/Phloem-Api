using MediatR;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Handles <see cref="GetAiPromptQuery"/>.</summary>
public sealed class GetAiPromptQueryHandler(IAiPromptVersionRepository versions, IUserProfileRepository profiles)
    : IRequestHandler<GetAiPromptQuery, Result<AiPromptDetailsDto>>
{
    /// <summary>The definition from code plus every saved version (newest first).</summary>
    /// <returns>The prompt; or <see cref="AiErrors.PromptNotFound"/>.</returns>
    public async Task<Result<AiPromptDetailsDto>> Handle(GetAiPromptQuery query, CancellationToken ct)
    {
        var p = AiPrompts.Find(query.Key.Trim());
        if (p is null) return AiErrors.PromptNotFound;

        var saved = await versions.GetVersionsAsync(p.Key, ct);
        var names = await profiles.GetFullNamesAsync(saved.Select(v => v.SavedBy).Distinct().ToList(), ct);
        var current = saved.Count > 0 ? saved[0] : null;

        return new AiPromptDetailsDto(
            p.Key, p.Name, p.Description, p.InputDescription, p.RequiredFields, p.SampleAnswer,
            current?.Text ?? p.DefaultPrompt, current?.Version ?? 0, p.DefaultPrompt, AiPromptVersion.TextMaxLength,
            saved.Select(v => new AiPromptVersionDto(v.Version, v.Text, v.Note, v.CreatedAt,
                                                     names.GetValueOrDefault(v.SavedBy, "Unknown user"))).ToList());
    }
}
