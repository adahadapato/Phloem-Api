using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Moderation.Commands;

/// <summary>Admin → Moderation: approve (a dish becomes visible) or reject (the seller sees the reason) an item.</summary>
/// <param name="Id">The check.</param>
/// <param name="Approve">True = fine; false = not allowed.</param>
/// <param name="Reason">Why it isn't allowed (required to reject; for dishes the seller sees it).</param>
public sealed record DecideModerationCommand(Guid Id, bool Approve, string? Reason) : IRequest<Result>;

/// <summary>The background checker: asks the AI about waiting checks (every minute).</summary>
/// <param name="Max">Most checks in one go.</param>
/// <returns>How many were finished.</returns>
public sealed record RunModerationCommand(int Max = 20) : IRequest<Result<int>>;
