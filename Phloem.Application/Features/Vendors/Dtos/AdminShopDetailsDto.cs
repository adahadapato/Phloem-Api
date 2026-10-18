using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>Admin → one shop: its details, pictures, documents, and what's still needed for the ✓ badge.</summary>
public sealed record AdminShopDetailsDto(
    AdminShopDto Shop,
    string? LogoUrl,
    string? CoverPhotoUrl,
    IReadOnlyList<AdminShopDocumentDto> Documents,
    IReadOnlyList<VendorDocumentType> MissingDocuments)
{
    /// <summary>Builds the review screen for one shop.</summary>
    public static AdminShopDetailsDto From(Vendor v, string ownerName, IFileStorage storage, DateTime utcNow) => new(
        AdminShopDto.From(v, ownerName),
        v.Logo is null ? null : storage.GetPublicUrl(v.Logo.Path),
        v.CoverPhoto is null ? null : storage.GetPublicUrl(v.CoverPhoto.Path),
        v.Documents.OrderBy(d => d.Type).Select(d => AdminShopDocumentDto.From(d, utcNow)).ToList(),
        v.MissingDocuments(utcNow));
}
