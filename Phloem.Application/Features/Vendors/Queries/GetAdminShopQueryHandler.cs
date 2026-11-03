using Phloem.Domain.Enums;
using Phloem.Application.Features.Moderation.Dtos;
using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetAdminShopQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IUserProfileRepository profiles,
    IFileStorage storage,
    IModerationCheckRepository checks,
    TimeProvider time)
    : IRequestHandler<GetAdminShopQuery, Result<AdminShopDetailsDto>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<AdminShopDetailsDto>> Handle(GetAdminShopQuery query, CancellationToken ct)
    {
        // GetByIdAsync includes the documents
        var vendor = await vendors.GetByIdAsync(query.ShopId, ct);
        // Not found, or a shop in a country this person doesn't cover: answer the same way
        if (vendor is null || !currentUser.CoversCountry(vendor.Country)) return VendorErrors.NotFound;

        var owners = await profiles.GetFullNamesAsync([vendor.OwnerId], ct);
        var nameCheck = (await checks.GetLatestAsync(ModerationKind.ShopName, [vendor.Id], ct)).GetValueOrDefault(vendor.Id);
        return AdminShopDetailsDto.From(vendor, owners.GetValueOrDefault(vendor.OwnerId, UnknownUser), storage,
                                        time.GetUtcNow().UtcDateTime,
                                        nameCheck is null ? null : ModerationAdviceDto.From(nameCheck));
    }
}
