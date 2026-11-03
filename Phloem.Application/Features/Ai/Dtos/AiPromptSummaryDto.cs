namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>One AI prompt in the list on Admin → AI.</summary>
/// <param name="Key">Stable id, e.g. "prompt.rewrite".</param>
/// <param name="Name">E.g. "Rewrite a prompt".</param>
/// <param name="Description">What the job does.</param>
/// <param name="Version">The version in use; 0 = Phloem's default.</param>
/// <param name="ChangedAt">When the version in use was saved; null for the default.</param>
/// <param name="ChangedBy">Who saved it; null for the default.</param>
public sealed record AiPromptSummaryDto(
    string Key,
    string Name,
    string Description,
    int Version,
    DateTime? ChangedAt,
    string? ChangedBy);
