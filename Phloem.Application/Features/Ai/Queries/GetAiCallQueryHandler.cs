using MediatR;
using Phloem.Application.Features.Ai.Common;
using Phloem.Application.Features.Ai.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Ai.Queries;

/// <summary>Handles <see cref="GetAiCallQuery"/>.</summary>
public sealed class GetAiCallQueryHandler(
    ICurrentUser currentUser,
    IAiCallLogRepository calls,
    IUserProfileRepository profiles,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetAiCallQuery, Result<AiCallDetailsDto>>
{
    /// <summary>Loads the call, records the view, returns it.</summary>
    /// <returns>The call; or <see cref="AiErrors.CallNotFound"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<AiCallDetailsDto>> Handle(GetAiCallQuery query, CancellationToken ct)
    {
        var staffId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var call = await calls.GetByIdAsync(query.Id, ct);
        if (call is null) return AiErrors.CallNotFound;

        audit.Add(StaffAuditEntry.Create(staffId, StaffAction.AiCallViewed, $"AI call {call.Id}",
                                         detail: call.Subject is null ? call.PromptKey : $"{call.PromptKey}: {call.Subject}"));
        await unitOfWork.SaveChangesAsync(ct);

        string? decidedBy = null;
        if (call.DecidedBy is { } by)
            decidedBy = (await profiles.GetFullNamesAsync([by], ct)).GetValueOrDefault(by, "Unknown user");

        return new AiCallDetailsDto(call.ToSummary(), call.Input, call.Output, call.Error, decidedBy, call.DecidedAtUtc);
    }
}
