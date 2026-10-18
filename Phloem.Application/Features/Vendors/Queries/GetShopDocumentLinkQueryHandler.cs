using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>
/// Signs a link that works for <see cref="LinkLifetime"/>; after that it's useless, so a forwarded or leaked link
/// quickly stops working.
/// </summary>
public sealed class GetShopDocumentLinkQueryHandler(IVendorRepository vendors, IFileStorage storage)
    : IRequestHandler<GetShopDocumentLinkQuery, Result<string>>
{
    /// <summary>How long a document link works: long enough to open it, short enough not to be shared.</summary>
    private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(10);

    public async Task<Result<string>> Handle(GetShopDocumentLinkQuery query, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(query.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var document = vendor.Documents.FirstOrDefault(d => d.Id == query.DocumentId);
        if (document is null) return VendorErrors.DocumentNotFound;

        var link = await storage.GetTemporaryUrlAsync(document.File.Path, LinkLifetime, ct);
        return link.ToString();
    }
}
