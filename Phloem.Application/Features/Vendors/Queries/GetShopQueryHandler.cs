using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>
/// A shop that isn't visible to buyers (waiting, rejected, blocked, closed) is reported as NOT FOUND, not
/// "forbidden", so its existence isn't revealed.
/// </summary>
public sealed class GetShopQueryHandler(
    IVendorRepository vendors,
    IDishRepository dishes,
    IFileStorage storage)
    : IRequestHandler<GetShopQuery, Result<ShopDetailsDto>>
{
    public async Task<Result<ShopDetailsDto>> Handle(GetShopQuery query, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(query.ShopId, ct);
        if (vendor is null || !vendor.IsVisibleToBuyers) return VendorErrors.NotFound;

        var counts = await dishes.CountByVendorsAsync([vendor.Id], ct);
        return ShopDetailsDto.From(vendor, counts.GetValueOrDefault(vendor.Id), storage);
    }
}
