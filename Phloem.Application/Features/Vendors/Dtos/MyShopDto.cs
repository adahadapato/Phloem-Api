using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>Shop settings + Seller dashboard header: the signed-in seller's own shop.</summary>
/// <remarks><c>MissingDocuments</c> lists required documents that aren't accepted yet (missing, pending, rejected or
/// expired): the app shows them as a to-do list for the ✓ badge.</remarks>
public sealed record MyShopDto(
    Guid Id,
    string Name,
    string BusinessEmail,
    string? BusinessPhone,
    Country Country,
    Currency Currency,
    VendorStatus Status,
    string? RejectionReason,
    bool IsLive,
    bool IsVerified,
    string? LogoUrl,
    string? CoverPhotoUrl,
    decimal? DeliveryFee,
    decimal Rating,
    int RatingCount,
    decimal Balance,
    DateTime OpenedAt,
    IReadOnlyList<MyShopDocumentDto> Documents,
    IReadOnlyList<VendorDocumentType> MissingDocuments,
    TimeOnly OrderCutoffTime)
{
    public static MyShopDto From(Vendor v, IFileStorage storage, DateTime utcNow) => new(
        v.Id,
        v.Name,
        v.BusinessEmail.Value,
        v.BusinessPhone?.Value,
        v.Country,
        v.Currency,
        v.Status,
        v.RejectionReason,
        v.IsVisibleToBuyers,
        v.IsVerified,
        v.Logo is null ? null : storage.GetPublicUrl(v.Logo.Path),
        v.CoverPhoto is null ? null : storage.GetPublicUrl(v.CoverPhoto.Path),
        v.DeliveryFee,
        v.Rating,
        v.RatingCount,
        v.Balance,
        v.CreatedAt,
        v.Documents.OrderBy(d => d.Type).Select(d => MyShopDocumentDto.From(d, utcNow)).ToList(),
        v.MissingDocuments(utcNow),
        v.OrderCutoffTime);
}