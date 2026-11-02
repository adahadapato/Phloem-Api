namespace Phloem.Application.Features.Staff.Dtos;

/// <summary>One staff job role and what it's allowed to do.</summary>
/// <param name="Role">The stored name, e.g. "ShipmentManager" (used in URLs).</param>
/// <param name="Name">As people read it, e.g. "Parcel hub".</param>
/// <param name="Description">What the role is for.</param>
/// <param name="Permissions">The permissions it holds, e.g. "parcels.manage".</param>
/// <param name="MemberCount">How many people have it.</param>
public sealed record JobRoleDto(
    string Role,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions,
    int MemberCount);
