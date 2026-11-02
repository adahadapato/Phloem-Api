using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Commands;

/// <summary>
/// Admin → Staff → Job roles: replaces what a job role is allowed to do (the permission tick-boxes).
/// </summary>
/// <param name="Role">The job role's stored name, e.g. "ShipmentManager".</param>
/// <param name="Permissions">Exactly the permissions it should hold (may be empty).</param>
/// <remarks>
/// <para><b>Admin only</b> (the owners), even for people with <c>staff.manage</c>.</para>
/// <para>Added permissions reach the role's members at their next token refresh. When any permission is TAKEN AWAY,
/// every member is signed out everywhere so it stops working at once. Written to the audit log.</para>
/// </remarks>
public sealed record SetJobRolePermissionsCommand(string Role, IReadOnlyList<string> Permissions) : IRequest<Result>;
