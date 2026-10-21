using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Admin suspends someone who abuses Phloem, until a date or until further notice:
/// <list type="bullet">
///   <item><description>they can't sign in, and every signed-in device is signed out (refresh tokens revoked; an
///   access token already issued stops working within its short lifetime);</description></item>
///   <item><description>their shop (if any) is hidden with them, and its orders that haven't left the shop are
///   cancelled (paid ones flagged for a refund);</description></item>
///   <item><description>their own orders as a buyer carry on;</description></item>
///   <item><description>they're emailed the reason and the end date.</description></item>
/// </list>
/// Admins can't be suspended, and nobody can suspend themselves.
/// </summary>
/// <param name="UserId">The person.</param>
/// <param name="Reason">Why (shown to them).</param>
/// <param name="UntilUtc">When it ends; null = until further notice.</param>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record SuspendUserCommand(Guid UserId, string Reason, DateTime? UntilUtc)
    : IRequest<Result<UserSuspendedDto>>;
