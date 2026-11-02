using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>
/// Signs a link that works for <see cref="LinkLifetime"/>; after that it's useless, so a forwarded or leaked link
/// quickly stops working.
/// </summary>
/// <remarks>Shop documents hold people's IDs and licences, so every opening is written to the staff audit log.</remarks>
public sealed class GetShopDocumentLinkQueryHandler(ICurrentUser currentUser, IVendorRepository vendors,
                                                    IFileStorage storage, IStaffAuditRepository audit,
                                                    IUnitOfWork unitOfWork)
    : IRequestHandler<GetShopDocumentLinkQuery, Result<string>>
{
    /// <summary>How long a document link works: long enough to open it, short enough not to be shared.</summary>
    private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(10);

    public async Task<Result<string>> Handle(GetShopDocumentLinkQuery query, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(query.ShopId, ct);
        // Not found, or a shop in a country this person doesn't cover: answer the same way
        if (vendor is null || !currentUser.CoversCountry(vendor.Country)) return VendorErrors.NotFound;

        var document = vendor.Documents.FirstOrDefault(d => d.Id == query.DocumentId);
        if (document is null) return VendorErrors.DocumentNotFound;

        if (currentUser.UserId is { } actorId)
        {
            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.ShopDocumentOpened, $"Shop {vendor.Name}",
                                             vendor.Country, $"Document: {document.Type}"));
            await unitOfWork.SaveChangesAsync(ct);
        }

        var link = await storage.GetTemporaryUrlAsync(document.File.Path, LinkLifetime, ct);
        return link.ToString();
    }
}
