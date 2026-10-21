using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>
/// Admin → Shops tab rows: "Mama Ama's Kitchen · owned by Ama Mensah · Ghana · waiting for approval".
/// Id is for the Approve / Reject / Block buttons. <see cref="IsOwnerSuspended"/>: hidden because its owner is
/// suspended (its own Status is unchanged).
/// </summary>
public sealed record AdminShopDto(
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
    DateTime OpenedAt,
    string? LogoUrl,
    bool IsOwnerSuspended)
{
    /// <summary>Builds the row from a shop, its owner's full name, and storage (for the logo's address).</summary>
    public static AdminShopDto From(Vendor v, string ownerName, IFileStorage storage) => new(
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
        v.CreatedAt,
        v.Logo is null ? null : storage.GetPublicUrl(v.Logo.Path),   // the logo on the admin seller cards
        v.IsOwnerSuspended);
}
