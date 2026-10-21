using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Admin lifts a suspension: the person can sign in again, and their shop (if any) comes back as it was (a shop that
/// was also blocked stays blocked). They're emailed. Lifting a suspension that isn't there is fine.
/// </summary>
/// <param name="UserId">The person.</param>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record ReinstateUserCommand(Guid UserId) : IRequest<Result>;
