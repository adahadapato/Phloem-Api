using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Common;

/// <summary>
/// Saves a new wording of a prompt (shared by Save, Restore and Reset): checks it still asks for every required
/// field, adds the next version, writes the audit log, saves, and drops the cached wording so jobs use it at once.
/// </summary>
/// <remarks>One step, logged (decided 2 Nov 2026), like prices. Every earlier wording stays and can be restored.</remarks>
public sealed class AiPromptSaving(
    IAiPromptVersionRepository versions,
    IAiPromptProvider prompts,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork)
{
    /// <summary>Saves the wording as the next version.</summary>
    /// <param name="definition">The job.</param>
    /// <param name="text">The new wording.</param>
    /// <param name="note">Why (optional).</param>
    /// <param name="actorId">Who.</param>
    /// <param name="action">Saved, restored or reset (for the audit log).</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The new version number; or <see cref="AiErrors.MissingFields"/> / <see cref="AiErrors.NoChange"/>.</returns>
    public async Task<Result<int>> SaveAsync(AiPromptDefinition definition, string text, string? note, Guid actorId,
                                             StaffAction action, CancellationToken ct)
    {
        var wording = text.Trim();
        var missing = AiPrompts.MissingFields(definition, wording);
        if (missing.Count > 0) return AiErrors.MissingFields(missing);

        var latest = await versions.GetLatestAsync(definition.Key, ct);
        var inUse = latest?.Text ?? definition.DefaultPrompt;
        if (string.Equals(inUse.Trim(), wording, StringComparison.Ordinal)) return AiErrors.NoChange;

        var next = (latest?.Version ?? 0) + 1;
        versions.Add(AiPromptVersion.Create(definition.Key, next, wording, note, actorId));
        audit.Add(StaffAuditEntry.Create(actorId, action, $"AI prompt {definition.Name}",
                                         detail: $"Version {next}{(string.IsNullOrWhiteSpace(note) ? "" : $": {note.Trim()}")}"));
        await unitOfWork.SaveChangesAsync(ct);

        await prompts.ForgetAsync(definition.Key, ct);
        return next;
    }
}
