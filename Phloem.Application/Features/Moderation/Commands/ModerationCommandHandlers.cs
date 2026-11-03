using MediatR;
using Phloem.Application.Features.Moderation.Common;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Moderation.Commands;

/// <summary>Handles <see cref="DecideModerationCommand"/>.</summary>
public sealed class DecideModerationCommandHandler(
    ICurrentUser currentUser,
    IModerationCheckRepository checks,
    IDishRepository dishes,
    IVendorRepository vendors,
    IAiCallLogRepository aiCalls,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<DecideModerationCommand, Result>
{
    /// <summary>Applies the decision, records it against the AI's answer (training data later) and in the audit log.</summary>
    /// <returns>Success; or <see cref="ModerationErrors.NotFound"/> / <see cref="ModerationErrors.NoLongerWaiting"/>.</returns>
    public async Task<Result> Handle(DecideModerationCommand command, CancellationToken ct)
    {
        var staffId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var check = await checks.GetByIdAsync(command.Id, ct);
        if (check is null || (check.Country is { } country && !currentUser.CoversCountry(country)))
            return ModerationErrors.NotFound;
        if (check.State != ModerationState.WaitingForPerson) return ModerationErrors.NoLongerWaiting;

        var now = time.GetUtcNow().UtcDateTime;
        string target;

        if (check.Kind == ModerationKind.Dish)
        {
            var dish = await dishes.GetByIdAsync(check.TargetId, ct);
            if (dish is null || dish.ModerationText != check.Text)
            {
                check.Replace();
                await unitOfWork.SaveChangesAsync(ct);
                return ModerationErrors.NoLongerWaiting;
            }
            if (command.Approve) dish.ApproveText(); else dish.RejectText(command.Reason!);
            target = $"Dish {dish.Name} ({dish.Vendor.Name})";
        }
        else
        {
            var shop = check.Kind == ModerationKind.ShopName ? await vendors.GetByIdAsync(check.TargetId, ct) : null;
            target = shop is null ? $"{check.Kind} {check.Text}" : $"Shop name {shop.Name}";
        }

        check.Decide(command.Approve, command.Reason, staffId, now);

        if (check.AiCallId is { } callId && await aiCalls.GetByIdAsync(callId, ct) is { } call)
            call.RecordDecision(command.Approve ? "Approved by staff" : $"Rejected by staff: {command.Reason}", staffId, now);

        audit.Add(StaffAuditEntry.Create(staffId,
                                         command.Approve ? StaffAction.ContentApproved : StaffAction.ContentRejected,
                                         target, check.Country, command.Approve ? null : command.Reason));
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Handles <see cref="RunModerationCommand"/>.</summary>
public sealed class RunModerationCommandHandler(ModerationRunner runner) : IRequestHandler<RunModerationCommand, Result<int>>
{
    /// <returns>How many checks were finished.</returns>
    public async Task<Result<int>> Handle(RunModerationCommand command, CancellationToken ct) =>
        await runner.RunAsync(Math.Clamp(command.Max, 1, 100), ct);
}
