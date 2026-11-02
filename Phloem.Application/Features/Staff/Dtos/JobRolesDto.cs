namespace Phloem.Application.Features.Staff.Dtos;

/// <summary>Admin → Staff → Job roles: every job role, plus every permission there is (for the tick-boxes).</summary>
/// <param name="Roles">The job roles, in the usual order.</param>
/// <param name="AllPermissions">Every permission Phloem knows, in the order the page lists them.</param>
/// <param name="CanEdit">True only for admins: only they can change what a job role is allowed to do.</param>
public sealed record JobRolesDto(
    IReadOnlyList<JobRoleDto> Roles,
    IReadOnlyList<string> AllPermissions,
    bool CanEdit);
