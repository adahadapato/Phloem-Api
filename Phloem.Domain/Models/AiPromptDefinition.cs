namespace Phloem.Domain.Models;

/// <summary>
/// One AI job's prompt as Phloem ships it (listed in <see cref="Constants.AiPrompts"/>). Staff can change the wording on
/// Admin → AI; these parts stay fixed in code.
/// </summary>
/// <param name="Key">Stable id, e.g. "prompt.rewrite" (never renamed: saved versions and the log use it).</param>
/// <param name="Name">What staff see, e.g. "Rewrite a prompt".</param>
/// <param name="Description">What the job is for and where its answer is used.</param>
/// <param name="InputDescription">What Phloem sends with the prompt (shown to whoever edits it).</param>
/// <param name="RequiredFields">The JSON fields the answer must have. The wording must mention each one, so an edit
/// can't break the job; answers missing one are refused.</param>
/// <param name="DefaultPrompt">Phloem's wording, used until staff save their own (and after "Reset").</param>
/// <param name="MaxOutputTokens">Longest answer allowed (keeps cost predictable).</param>
/// <param name="SampleAnswer">A correct JSON answer: what the Fake provider returns, and a guide for editors.</param>
/// <param name="ShowOnPromptsPage">False for helper prompts staff don't need to see (e.g. the one behind "Ask AI to
/// improve this prompt"); they keep their default wording.</param>
public sealed record AiPromptDefinition(
    string Key,
    string Name,
    string Description,
    string InputDescription,
    IReadOnlyList<string> RequiredFields,
    string DefaultPrompt,
    int MaxOutputTokens,
    string SampleAnswer,
    bool ShowOnPromptsPage = true);
