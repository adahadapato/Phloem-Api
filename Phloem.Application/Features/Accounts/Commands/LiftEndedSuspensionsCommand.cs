using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Sent every hour by the suspension expiry task (Infrastructure): lifts suspensions whose end date has passed, so
/// their shops come back by themselves. Suspensions "until further notice" are left for an admin.
/// </summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record LiftEndedSuspensionsCommand : IRequest<Result<int>>;
