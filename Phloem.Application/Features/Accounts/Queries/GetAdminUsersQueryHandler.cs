using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>
/// Handles <see cref="GetAdminUsersQuery"/>: one page of profiles, plus each person's email, roles and shop name.
/// </summary>
/// <remarks>Email and roles live on the login account, so they're read per person (one page at a time).</remarks>
public sealed class GetAdminUsersQueryHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IAccountService accounts,
    IUserRoleService roles,
    IVendorRepository vendors,
    TimeProvider time)
    : IRequestHandler<GetAdminUsersQuery, Result<PagedResult<AdminUserDto>>>
{
    public async Task<Result<PagedResult<AdminUserDto>>> Handle(GetAdminUsersQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await profiles.GetPagedAsync(query.Search?.Trim(), page, ct, query.SuspendedOnly);

        var ids = paged.Items.Select(p => p.Id).ToList();
        var shopNames = await vendors.GetNamesByOwnerIdsAsync(ids, ct);
        var now = time.GetUtcNow().UtcDateTime;

        var items = new List<AdminUserDto>(paged.Items.Count);
        foreach (var p in paged.Items)
        {
            var contact = await accounts.GetContactAsync(p.Id, ct);
            var userRoles = await roles.GetRolesAsync(p.Id, ct);
            var isAdmin = userRoles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase);
            var isSuspended = p.IsSuspendedAt(now);

            items.Add(new AdminUserDto(
                p.Id,
                p.GetFullName(),
                contact.IsSuccess ? contact.Value.Email : string.Empty,
                contact.IsSuccess ? contact.Value.PhoneNumber : null,
                p.Country,
                userRoles,
                p.CreatedAt,
                shopNames.GetValueOrDefault(p.Id),
                isSuspended,
                isSuspended ? p.SuspendedUntilUtc : null,
                isSuspended ? p.SuspensionReason : null,
                CanBeSuspended: !isAdmin && p.Id != currentUser.UserId));
        }

        return new PagedResult<AdminUserDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
