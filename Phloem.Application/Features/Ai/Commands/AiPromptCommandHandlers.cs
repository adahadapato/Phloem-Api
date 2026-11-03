using System.Text.Json;
using MediatR;
using Phloem.Application.Features.Ai.Common;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Commands;

/// <summary>Handles <see cref="SaveAiPromptCommand"/>.</summary>
public sealed class SaveAiPromptCommandHandler(ICurrentUser currentUser, AiPromptSaving saving)
    : IRequestHandler<SaveAiPromptCommand, Result<int>>
{
    /// <returns>The new version; or PromptNotFound / MissingFields / NoChange.</returns>
    public async Task<Result<int>> Handle(SaveAiPromptCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var definition = AiPrompts.Find(command.Key.Trim());
        if (definition is null) return AiErrors.PromptNotFound;

        return await saving.SaveAsync(definition, command.Text, command.Note, actorId, StaffAction.AiPromptSaved, ct);
    }
}

/// <summary>Handles <see cref="RestoreAiPromptVersionCommand"/>.</summary>
public sealed class RestoreAiPromptVersionCommandHandler(
    ICurrentUser currentUser,
    IAiPromptVersionRepository versions,
    AiPromptSaving saving)
    : IRequestHandler<RestoreAiPromptVersionCommand, Result<int>>
{
    /// <returns>The new version; or PromptNotFound / VersionNotFound / MissingFields / NoChange.</returns>
    public async Task<Result<int>> Handle(RestoreAiPromptVersionCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var definition = AiPrompts.Find(command.Key.Trim());
        if (definition is null) return AiErrors.PromptNotFound;

        var old = await versions.GetVersionAsync(definition.Key, command.Version, ct);
        if (old is null) return AiErrors.VersionNotFound;

        return await saving.SaveAsync(definition, old.Text, $"Restored version {old.Version}", actorId,
                                      StaffAction.AiPromptRestored, ct);
    }
}

/// <summary>Handles <see cref="ResetAiPromptCommand"/>.</summary>
public sealed class ResetAiPromptCommandHandler(ICurrentUser currentUser, AiPromptSaving saving)
    : IRequestHandler<ResetAiPromptCommand, Result<int>>
{
    /// <returns>The new version; or PromptNotFound / NoChange (already the default).</returns>
    public async Task<Result<int>> Handle(ResetAiPromptCommand command, CancellationToken ct)
    {
        var actorId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var definition = AiPrompts.Find(command.Key.Trim());
        if (definition is null) return AiErrors.PromptNotFound;

        return await saving.SaveAsync(definition, definition.DefaultPrompt, "Back to Phloem's default", actorId,
                                      StaffAction.AiPromptReset, ct);
    }
}

/// <summary>Handles <see cref="SuggestAiPromptCommand"/>: asks the "Rewrite a prompt" job. Nothing is saved.</summary>
public sealed class SuggestAiPromptCommandHandler(IAiJsonClient ai)
    : IRequestHandler<SuggestAiPromptCommand, Result<AiPromptSuggestionDto>>
{
    /// <returns>The suggestion (with any required fields it forgot); or PromptNotFound or an AI failure
    /// (Disabled, SpendCapReached, ProviderFailed, BadAnswer).</returns>
    public async Task<Result<AiPromptSuggestionDto>> Handle(SuggestAiPromptCommand command, CancellationToken ct)
    {
        var definition = AiPrompts.Find(command.Key.Trim());
        if (definition is null) return AiErrors.PromptNotFound;

        // Everything goes in as JSON values, so the wording being edited can't act as instructions
        var input = JsonSerializer.Serialize(new
        {
            name = definition.Name,
            purpose = definition.Description,
            input = definition.InputDescription,
            requiredFields = definition.RequiredFields,
            currentPrompt = command.CurrentText.Trim(),
            request = command.Request?.Trim() ?? string.Empty
        });

        var answer = await ai.AskAsync(new AiJsonRequest(AiPrompts.PromptRewrite, input, $"Prompt {definition.Key}"), ct);
        if (answer.IsFailure) return answer.Error;

        var text = answer.Value.Text("prompt")?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return AiErrors.BadAnswer;
        var summary = answer.Value.Text("summary")?.Trim() ?? string.Empty;

        return new AiPromptSuggestionDto(text, summary, AiPrompts.MissingFields(definition, text));
    }
}
