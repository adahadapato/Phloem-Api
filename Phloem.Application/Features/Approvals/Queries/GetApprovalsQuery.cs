using MediatR;
using Phloem.Application.Features.Approvals.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Approvals.Queries;

/// <summary>Which list on Admin → Approvals.</summary>
public enum ApprovalsView
{
    /// <summary>Changes waiting for the viewer to decide (not their own).</summary>
    Waiting = 1,

    /// <summary>Changes the viewer proposed, newest first (waiting ones included).</summary>
    Mine = 2,

    /// <summary>Decided, withdrawn and expired changes the viewer may see, newest first.</summary>
    History = 3
}

/// <summary>Admin → Approvals: one of the three lists, one page.</summary>
/// <param name="View">Which list.</param>
/// <param name="Page">Page number, from 1.</param>
/// <param name="PageSize">Lines per page.</param>
public sealed record GetApprovalsQuery(ApprovalsView View = ApprovalsView.Waiting, int Page = 1,
                                       int PageSize = PageRequest.DefaultPageSize)
    : IRequest<Result<PagedResult<PendingChangeDto>>>;

/// <summary>How many changes are waiting for the viewer (the number next to Approvals in the menu).</summary>
public sealed record GetApprovalCountQuery : IRequest<Result<int>>;
