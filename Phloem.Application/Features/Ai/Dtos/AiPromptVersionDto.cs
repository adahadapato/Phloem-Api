namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>One saved wording of a prompt.</summary>
/// <param name="Version">1, 2, 3…</param>
/// <param name="Text">The wording.</param>
/// <param name="Note">Why it changed.</param>
/// <param name="SavedAt">When.</param>
/// <param name="SavedBy">Who (full name).</param>
public sealed record AiPromptVersionDto(int Version, string Text, string? Note, DateTime SavedAt, string SavedBy);
