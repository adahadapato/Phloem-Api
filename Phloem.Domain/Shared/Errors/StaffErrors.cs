namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for staff access (roles, permissions, countries). Nothing here is thrown.</summary>
public static class StaffErrors
{
    /// <summary>Only an Admin (an owner) can give or take the Admin role.</summary>
    public static readonly Error OnlyAdminCanChangeAdmin = Error.Forbidden(
        "Staff.OnlyAdminCanChangeAdmin", "Only an admin can give or take the Admin role.");

    /// <summary>Only an Admin can change what a job role is allowed to do.</summary>
    public static readonly Error OnlyAdminCanChangeRolePermissions = Error.Forbidden(
        "Staff.OnlyAdminCanChangeRolePermissions", "Only an admin can change what a job role is allowed to do.");

    /// <summary>Staff can't change their own roles or countries; someone else must do it.</summary>
    public static readonly Error CannotChangeOwnAccess = Error.Forbidden(
        "Staff.CannotChangeOwnAccess", "You can't change your own access. Ask an admin to do it.");

    /// <summary>The setting or person is in a country the staff member doesn't cover.</summary>
    public static readonly Error CountryNotCovered = Error.Forbidden(
        "Staff.CountryNotCovered", "That country isn't one of yours.");

    /// <summary>Countries only apply to people with a staff job role (admins cover every country).</summary>
    public static readonly Error NotStaff = Error.Validation(
        "Staff.NotStaff", "Give this person a job role first. Admins already cover every country.");

    /// <summary>The role named isn't a staff job role.</summary>
    public static readonly Error NotAJobRole = Error.Validation(
        "Staff.NotAJobRole", "That isn't a staff job role.");

    /// <summary>A permission in the list isn't one Phloem knows.</summary>
    public static readonly Error UnknownPermission = Error.Validation(
        "Staff.UnknownPermission", "One of the permissions isn't recognised. Please reload the page.");
}
