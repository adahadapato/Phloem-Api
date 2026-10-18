using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetAdminShopQueryHandler(
    IVendorRepository vendors,
    IUserProfileRepository profiles,
    IFileStorage storage,
    TimeProvider time)
    : IRequestHandler<GetAdminShopQuery, Result<AdminShopDetailsDto>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<AdminShopDetailsDto>> Handle(GetAdminShopQuery query, CancellationToken ct)
    {
        // GetByIdAsync includes the documents
        var vendor = await vendors.GetByIdAsync(query.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var owners = await profiles.GetFullNamesAsync([vendor.OwnerId], ct);
        return AdminShopDetailsDto.From(vendor, owners.GetValueOrDefault(vendor.OwnerId, UnknownUser), storage,
                                        time.GetUtcNow().UtcDateTime);
    }
}
