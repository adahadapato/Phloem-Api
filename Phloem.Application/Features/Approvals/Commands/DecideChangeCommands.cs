using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Approvals.Commands;

/// <summary>A second person approves a change; it's applied now. Returns what happened, e.g. "Published as
/// version 6".</summary>
/// <param name="Id">The change.</param>
public sealed record ApproveChangeCommand(Guid Id) : IRequest<Result<string>>;

/// <summary>A second person rejects a change (a blocked shop is unblocked).</summary>
/// <param name="Id">The change.</param>
/// <param name="Reason">Why (shown to the proposer).</param>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record RejectChangeCommand(Guid Id, string Reason) : IRequest<Result>;

/// <summary>The proposer takes back a waiting Terms / Privacy proposal.</summary>
/// <param name="Id">The change.</param>
public sealed record WithdrawChangeCommand(Guid Id) : IRequest<Result>;

/// <summary>Background task: changes nobody decided in time expire (a block nobody confirmed is undone).</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record ExpirePendingChangesCommand : IRequest<Result<int>>;
