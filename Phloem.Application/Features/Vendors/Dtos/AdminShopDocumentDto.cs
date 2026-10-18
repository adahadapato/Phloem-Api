using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>One document on the admin's review screen (open it through <c>GetShopDocumentLinkQuery</c>).</summary>
public sealed record AdminShopDocumentDto(
    Guid Id,
    VendorDocumentType Type,
    DocumentStatus Status,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string? RejectionReason,
    DateTime? ExpiresAt,
    bool IsExpired,
    DateTime UploadedAt,
    DateTime? ReviewedAt)
{
    /// <summary>Builds the row; <paramref name="utcNow"/> decides whether it has expired.</summary>
    public static AdminShopDocumentDto From(VendorDocument d, DateTime utcNow) => new(
        d.Id,
        d.Type,
        d.Status,
        d.File.OriginalFileName,
        d.File.ContentType,
        d.File.FileSizeBytes,
        d.RejectionReason,
        d.ExpiresAt,
        d.IsExpired(utcNow),
        d.CreatedAt,
        d.ReviewedAt);
}
