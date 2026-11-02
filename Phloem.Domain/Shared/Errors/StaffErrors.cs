namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for staff access (roles, permissions, countries). Nothing here is thrown.</summary>
public static class StaffErrors
{
    /// <summary>Only an Admin (an owner) can give or take the Admin role.</summary>
    public static readonly Error OnlyAdminCanChangeAdmin = Error.Forbidden(
        "Staff.OnlyAdminCanChangeAdmin", "Only an admin can give or take the Admin role.");
}
