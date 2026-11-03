using MediatR;
using Phloem.Application.Features.Seo.Common;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Commands;

/// <summary>Handles <see cref="WriteSeoPageCommand"/>.</summary>
public sealed class WriteSeoPageCommandHandler(
    ICurrentUser currentUser,
    SeoWriter writer,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork)
    : IRequestHandler<WriteSeoPageCommand, Result>
{
    /// <returns>Success; or <see cref="SeoErrors.PageNotFound"/> or an AI failure (off, cap, service down).</returns>
    public async Task<Result> Handle(WriteSeoPageCommand command, CancellationToken ct)
    {
        var staffId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var key = command.Key.Trim();

        var input = SeoPages.FindMain(key) is { } main
            ? SeoWriter.MainInput(main)
            : SeoPages.ShopIdFromKey(key) is { } shopId ? await writer.ShopInputAsync(shopId, ct) : null;
        if (input is null) return SeoErrors.PageNotFound;

        var written = await writer.WriteAsync(input, staffId, ct);
        if (written.IsFailure) return written.Error;

        audit.Add(StaffAuditEntry.Create(staffId, StaffAction.SeoRewritten, $"SEO {input.Name}"));
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Handles <see cref="SaveSeoPageCommand"/>.</summary>
public sealed class SaveSeoPageCommandHandler(
    ICurrentUser currentUser,
    SeoPageLookup lookup,
    ISeoEntryRepository entries,
    IAiCallLogRepository aiCalls,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<SaveSeoPageCommand, Result>
{
    /// <summary>Saves staff's wording, and records "Edited by staff" against the AI's call (training data later).</summary>
    /// <returns>Success; or <see cref="SeoErrors.PageNotFound"/>.</returns>
    public async Task<Result> Handle(SaveSeoPageCommand command, CancellationToken ct)
    {
        var staffId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var key = command.Key.Trim();

        var page = (await lookup.AllAsync(ct)).FirstOrDefault(p => p.Row.Key == key);
        if (page is null) return SeoErrors.PageNotFound;

        var keywords = command.Keywords ?? [];
        var entry = await entries.GetByKeyAsync(key, ct);
        if (entry is null)
        {
            entries.Add(SeoEntry.WrittenByStaff(key, page.Row.Kind, page.Shop?.Id, command.Title, command.Description,
                                                keywords, staffId));
        }
        else
        {
            if (entry is { Source: SeoSource.Ai, AiCallId: { } callId }
                && await aiCalls.GetByIdAsync(callId, ct) is { } call)
                call.RecordDecision("Edited by staff", staffId, time.GetUtcNow().UtcDateTime);

            entry.EditByStaff(command.Title, command.Description, keywords, staffId);
        }

        audit.Add(StaffAuditEntry.Create(staffId, StaffAction.SeoEdited, $"SEO {page.Row.Name}",
                                         page.Row.Country, $"Title: {command.Title.Trim()}"));
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Handles <see cref="WriteMissingSeoCommand"/>.</summary>
public sealed class WriteMissingSeoCommandHandler(SeoWriter writer)
    : IRequestHandler<WriteMissingSeoCommand, Result<int>>
{
    /// <returns>How many were written; the AI failure when none could be written because of it (e.g. AI off).</returns>
    public async Task<Result<int>> Handle(WriteMissingSeoCommand command, CancellationToken ct)
    {
        var (written, stoppedBy) = await writer.WriteMissingAsync(Math.Clamp(command.Max, 1, 100), ct);
        if (written == 0 && stoppedBy is not null) return stoppedBy;
        return written;
    }
}
