using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Seller uploads a document for the admin to check (PDF, JPEG or PNG, up to 10 MB). One per type: a new upload
/// replaces the old one. Returns the new document's Id.
/// </summary>
/// <param name="Type">Which document it is.</param>
/// <param name="Content">The file.</param>
/// <param name="FileName">The file's name on the seller's device (shown to the admin).</param>
/// <param name="SizeBytes">The file's size.</param>
/// <param name="ExpiresOn">When the certificate expires; null if it doesn't.</param>
public sealed record UploadShopDocumentCommand(
    VendorDocumentType Type,
    Stream Content,
    string FileName,
    long SizeBytes,
    DateOnly? ExpiresOn) : IRequest<Result<Guid>>;