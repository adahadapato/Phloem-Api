using MediatR;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Queries;

/// <summary>Handles <see cref="GetJobRolesQuery"/>.</summary>
public sealed class GetJobRolesQueryHandler(ICurrentUser currentUser, IUserRoleService roles)
    : IRequestHandler<GetJobRolesQuery, Result<JobRolesDto>>
{
    /// <summary>Reads every job role's permissions and how many people hold it.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The roles, every permission, and whether the viewer may edit.</returns>
    public async Task<Result<JobRolesDto>> Handle(GetJobRolesQuery query, CancellationToken ct)
    {
        var items = new List<JobRoleDto>(Roles.JobRoles.Count);
        foreach (var role in Roles.JobRoles)
        {
            var permissions = await roles.GetRolePermissionsAsync(role, ct);
            var members = await roles.GetUserIdsInRoleAsync(role, ct);
            items.Add(new JobRoleDto(role, Roles.DisplayName(role), Roles.Description(role), permissions,
                                     members.Count));
        }

        return new JobRolesDto(items, Permissions.All, CanEdit: currentUser.IsInRole(Roles.Admin));
    }
}
