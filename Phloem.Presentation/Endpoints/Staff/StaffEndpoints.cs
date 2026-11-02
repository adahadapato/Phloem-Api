using MediatR;
using Phloem.Application.Features.Staff.Commands;
using Phloem.Application.Features.Staff.Dtos;
using Phloem.Application.Features.Staff.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authorization;

namespace Phloem.Presentation.Endpoints.Staff;

/// <summary>
/// Admin → Staff (<c>/api/admin/staff</c>) and the staff audit log (<c>/api/admin/audit</c>).
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description><b>Staff list, job roles, countries:</b> permission <c>staff.manage</c> (owners have it; they
///   may give it to others). Someone covering only some countries works within those countries.</description></item>
///   <item><description><b>What a job role may do:</b> Admin only (the owners), checked here AND in the
///   handler.</description></item>
///   <item><description><b>Audit log:</b> permission <c>audit.view</c>, every country.</description></item>
/// </list>
/// <para>Giving and taking roles stays on <c>/api/admin/users/{userId}/roles/{role}</c>.</para>
/// </remarks>
public static class StaffEndpoints
{
    /// <summary>Maps the staff endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapStaffEndpoints(this IEndpointRouteBuilder api)
    {
        MapStaff(api);
        MapAudit(api);
        return api;
    }

    // ───────────────────────────── 1. Staff ─────────────────────────────

    private static void MapStaff(IEndpointRouteBuilder api)
    {
        var staff = api.MapGroup("/admin/staff")
                       .WithTags("Admin: Staff")
                       .RequirePermission(Permissions.Staff.Manage, CountryRule.RecordCountry);

        staff.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetStaffQuery(), ct)).ToHttp())
            .WithName("GetStaff")
            .WithSummary("The owners and everyone with a job role: roles, countries, two-factor, last sign-in")
            .Produces<IReadOnlyList<StaffMemberDto>>();

        staff.MapPut("/{userId:guid}/countries",
                async (Guid userId, StaffCountriesRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new SetStaffCountriesCommand(userId, body.AllCountries, body.Countries), ct))
                    .ToHttp())
            .WithName("SetStaffCountries")
            .WithSummary("Set which countries a staff member covers (one, several or all)")
            .WithDescription("Signs them out everywhere so it applies at once. You can't change your own; if you " +
                             "cover only some countries you can only add or remove those, and can't give \"all\".")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        staff.MapGet("/job-roles", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetJobRolesQuery(), ct)).ToHttp())
            .WithName("GetJobRoles")
            .WithSummary("Every job role with its permissions and member count, plus every permission there is")
            .Produces<JobRolesDto>();

        staff.MapPut("/job-roles/{role}/permissions",
                async (string role, JobRolePermissionsRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new SetJobRolePermissionsCommand(role, body.Permissions ?? []), ct)).ToHttp())
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))   // owners only
            .WithName("SetJobRolePermissions")
            .WithSummary("Change what a job role is allowed to do (owners only)")
            .WithDescription("Exactly these permissions afterwards. Taking any away signs the role's members out " +
                             "everywhere so it stops working at once.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    // ───────────────────────────── 2. Audit log ─────────────────────────────

    private static void MapAudit(IEndpointRouteBuilder api)
    {
        api.MapGet("/admin/audit", async ([AsParameters] GetStaffAuditQuery query, ISender sender,
                                          CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .RequirePermission(Permissions.Platform.ViewAudit)
            .WithTags("Admin: Audit log")
            .WithName("GetStaffAudit")
            .WithSummary("Who did what, newest first: filter by person, action, country and dates")
            .Produces<PagedResult<StaffAuditEntryDto>>()
            .ProducesValidationProblem();
    }

    /// <summary>Body of "set countries".</summary>
    /// <param name="AllCountries">True = every country.</param>
    /// <param name="Countries">The countries when not all.</param>
    public sealed record StaffCountriesRequest(bool AllCountries, IReadOnlyList<Country>? Countries);

    /// <summary>Body of "set a job role's permissions".</summary>
    /// <param name="Permissions">Exactly the permissions the role should hold.</param>
    public sealed record JobRolePermissionsRequest(IReadOnlyList<string>? Permissions);
}
