using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>
/// Admin → Sellers tab rows: "Mama Ama's Kitchen · owned by Ama Mensah · Ghana · waiting for approval".
/// Id is only for the Approve / Reject / Block buttons.
/// </summary>
public sealed record AdminVendorDto(
    Guid Id,
    string Name,
    string OwnerName,
    string BusinessEmail,
    string? BusinessPhone,
    Country Country,
    VendorStatus Status,
    string? RejectionReason,
    bool IsVerified,
    decimal Rating,
    int RatingCount,
    DateTime OpenedAt)
{
    public static AdminVendorDto From(Vendor v, string ownerName) => new(
        v.Id,
        v.Name,
        ownerName,
        v.BusinessEmail.Value,
        v.BusinessPhone?.Value,
        v.Country,
        v.Status,
        v.RejectionReason,
        v.IsVerified,
        v.Rating,
        v.RatingCount,
        v.CreatedAt);
}