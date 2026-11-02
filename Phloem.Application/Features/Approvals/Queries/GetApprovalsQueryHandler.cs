using MediatR;
using Phloem.Application.Common.Authorization;
using Phloem.Application.Features.Approvals.Common;
using Phloem.Application.Features.Approvals.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Approvals.Queries;

/// <summary>Handles <see cref="GetApprovalsQuery"/> and <see cref="GetApprovalCountQuery"/>.</summary>
public sealed class GetApprovalsQueryHandler(
    ICurrentUser currentUser,
    IPendingChangeRepository changes,
    IUserProfileRepository profiles,
    TimeProvider time)
    : IRequestHandler<GetApprovalsQuery, Result<PagedResult<PendingChangeDto>>>,
      IRequestHandler<GetApprovalCountQuery, Result<int>>
{
    /// <summary>One page of the chosen list.</summary>
    public async Task<Result<PagedResult<PendingChangeDto>>> Handle(GetApprovalsQuery query, CancellationToken ct)
    {
        if (!currentUser.IsStaffMember()) return ApprovalErrors.StaffOnly;

        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var now = time.GetUtcNow().UtcDateTime;

        PagedResult<PendingChange> paged;
        if (query.View == ApprovalsView.Waiting)
        {
            // Few at a time, so filtered here: only what this person may decide
            var mine = (await WaitingForMeAsync(now, ct)).ToList();
            paged = new PagedResult<PendingChange>(mine.Skip(page.Skip).Take(page.PageSize).ToList(), mine.Count,
                                                   page.Page, page.PageSize);
        }
        else
        {
            var isOwner = currentUser.IsInRole(Roles.Admin);
            paged = await changes.GetHistoryAsync(
                proposedById: query.View == ApprovalsView.Mine ? currentUser.UserId : null,
                coveredCountries: query.View == ApprovalsView.Mine || isOwner ? null : currentUser.CountryLimit(),
                includeWaiting: query.View == ApprovalsView.Mine,
                page, ct);
        }

        var people = paged.Items.SelectMany(c => new[] { c.ProposedById, c.DecidedById ?? Guid.Empty })
                                .Where(id => id != Guid.Empty).Distinct().ToList();
        var names = await profiles.GetFullNamesAsync(people, ct);

        var items = paged.Items.Select(c => PendingChangeDto.From(
            c, names,
            canDecide: c.IsWaitingAt(now) && ApprovalRules.CanDecide(currentUser, c),
            canWithdraw: c.IsWaitingAt(now) && ApprovalRules.CanWithdraw(currentUser, c),
            currentUser.UserId)).ToList();

        return new PagedResult<PendingChangeDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    /// <summary>How many are waiting for this person.</summary>
    public async Task<Result<int>> Handle(GetApprovalCountQuery query, CancellationToken ct)
    {
        if (!currentUser.IsStaffMember()) return 0;
        return (await WaitingForMeAsync(time.GetUtcNow().UtcDateTime, ct)).Count();
    }

    private async Task<IEnumerable<PendingChange>> WaitingForMeAsync(DateTime now, CancellationToken ct) =>
        (await changes.GetWaitingAsync(ct)).Where(c => c.IsWaitingAt(now) && ApprovalRules.CanDecide(currentUser, c));
}
