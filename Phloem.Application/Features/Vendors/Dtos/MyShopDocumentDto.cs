using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>One of my shop's documents, as the seller sees it (the file itself is private: admins only).</summary>
public sealed record MyShopDocumentDto(
    Guid Id,
    VendorDocumentType Type,
    DocumentStatus Status,
    string OriginalFileName,
    string? RejectionReason,
    DateTime? ExpiresAt,
    bool IsExpired,
    DateTime UploadedAt)
{
    /// <summary>Maps a document.</summary>
    public static MyShopDocumentDto From(VendorDocument d, DateTime utcNow) => new(
        d.Id, d.Type, d.Status, d.File.OriginalFileName, d.RejectionReason, d.ExpiresAt, d.IsExpired(utcNow),
        d.CreatedAt);
}