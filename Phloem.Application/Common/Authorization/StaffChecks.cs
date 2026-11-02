using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Common.Authorization;

/// <summary>Who counts as staff, for handlers that are open to every staff member.</summary>
public static class StaffChecks
{
    /// <summary>True for the owners (Admin) and anyone with a staff job role.</summary>
    /// <param name="currentUser">The signed-in person.</param>
    public static bool IsStaffMember(this ICurrentUser currentUser) =>
        currentUser.IsInRole(Roles.Admin) || Roles.JobRoles.Any(currentUser.IsInRole);
}
