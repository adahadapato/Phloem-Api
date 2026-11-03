using MediatR;
using Phloem.Application.Features.Ai.Commands;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Application.Features.Ai.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authorization;

namespace Phloem.Presentation.Endpoints.Ai;

/// <summary>
/// Admin → AI (<c>/api/admin/ai</c>, permission <c>ai.prompts.manage</c>): status and spend, the prompts AI jobs
/// use (edit, restore, reset, "Ask AI to rewrite"), and the AI call log.
/// </summary>
/// <remarks>
/// <para>Prompt changes are one step and logged. Opening a call from the log is audited (it can hold people's
/// text). Not tied to a country.</para>
/// </remarks>
public static class AiEndpoints
{
    /// <summary>Maps the AI admin endpoints.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder api)
    {
        var ai = api.MapGroup("/admin/ai")
                    .WithTags("Admin: AI")
                    .RequirePermission(Permissions.Content.AiPrompts, CountryRule.NotCountrySpecific);

        ai.MapGet("/status", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAiStatusQuery(), ct)).ToHttp())
            .WithName("GetAiStatus")
            .WithSummary("On or off, the model, and this month's calls and spend against the cap")
            .Produces<AiStatusDto>();

        // ───────────── Prompts ─────────────

        ai.MapGet("/prompts", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAiPromptsQuery(), ct)).ToHttp())
            .WithName("GetAiPrompts")
            .WithSummary("Every AI prompt with the version in use")
            .Produces<IReadOnlyList<AiPromptSummaryDto>>();

        ai.MapGet("/prompts/{key}", async (string key, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAiPromptQuery(key), ct)).ToHttp())
            .WithName("GetAiPrompt")
            .WithSummary("One prompt: wording in use, default, required answer fields, saved versions")
            .Produces<AiPromptDetailsDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        ai.MapPut("/prompts/{key}", async (string key, SavePromptRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new SaveAiPromptCommand(key, body.Text, body.Note), ct)).ToHttp())
            .WithName("SaveAiPrompt")
            .WithSummary("Save a new wording (used at once); returns the new version number")
            .Produces<int>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        ai.MapPost("/prompts/{key}/versions/{version:int}/restore",
                async (string key, int version, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new RestoreAiPromptVersionCommand(key, version), ct)).ToHttp())
            .WithName("RestoreAiPromptVersion")
            .WithSummary("Bring back an older wording (saved as a new version)")
            .Produces<int>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        ai.MapPost("/prompts/{key}/reset", async (string key, ISender sender, CancellationToken ct) =>
                (await sender.Send(new ResetAiPromptCommand(key), ct)).ToHttp())
            .WithName("ResetAiPrompt")
            .WithSummary("Go back to Phloem's default wording (saved as a new version)")
            .Produces<int>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        ai.MapPost("/prompts/{key}/suggest",
                async (string key, SuggestPromptRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new SuggestAiPromptCommand(key, body.CurrentText, body.Request), ct)).ToHttp())
            .WithName("SuggestAiPrompt")
            .WithSummary("Ask AI to rewrite the wording; NOTHING is saved")
            .Produces<AiPromptSuggestionDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────── Call log ─────────────

        ai.MapGet("/calls", async ([AsParameters] GetAiCallsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetAiCalls")
            .WithSummary("The AI call log, newest first (no text); filter by job and outcome")
            .Produces<PagedResult<AiCallSummaryDto>>()
            .ProducesValidationProblem();

        ai.MapGet("/calls/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAiCallQuery(id), ct)).ToHttp())
            .WithName("GetAiCall")
            .WithSummary("One call with what went in and came back (the view is audited)")
            .Produces<AiCallDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }

    /// <summary>Body of "save a prompt".</summary>
    /// <param name="Text">The new wording.</param>
    /// <param name="Note">Why it changed (optional).</param>
    public sealed record SavePromptRequest(string Text, string? Note);

    /// <summary>Body of "Ask AI to rewrite".</summary>
    /// <param name="CurrentText">The wording in the editor now.</param>
    /// <param name="Request">What to change (optional).</param>
    public sealed record SuggestPromptRequest(string CurrentText, string? Request);
}
