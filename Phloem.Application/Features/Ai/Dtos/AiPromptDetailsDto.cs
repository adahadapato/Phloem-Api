namespace Phloem.Application.Features.Ai.Dtos;

/// <summary>One AI prompt for editing: the wording in use, Phloem's default, and every saved version.</summary>
/// <param name="Key">Stable id.</param>
/// <param name="Name">What staff see.</param>
/// <param name="Description">What the job does.</param>
/// <param name="InputDescription">What Phloem sends with the prompt.</param>
/// <param name="RequiredFields">Answer fields the wording must keep asking for.</param>
/// <param name="SampleAnswer">A correct answer, as a guide.</param>
/// <param name="Text">The wording in use.</param>
/// <param name="Version">Its version; 0 = Phloem's default.</param>
/// <param name="DefaultText">Phloem's default wording.</param>
/// <param name="MaxLength">Longest wording allowed.</param>
/// <param name="Versions">Saved versions, newest first.</param>
public sealed record AiPromptDetailsDto(
    string Key,
    string Name,
    string Description,
    string InputDescription,
    IReadOnlyList<string> RequiredFields,
    string SampleAnswer,
    string Text,
    int Version,
    string DefaultText,
    int MaxLength,
    IReadOnlyList<AiPromptVersionDto> Versions);
