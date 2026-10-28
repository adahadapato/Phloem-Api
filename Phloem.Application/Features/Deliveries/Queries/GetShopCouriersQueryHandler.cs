using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Queries;

/// <summary>Handles <see cref="GetShopCouriersQuery"/>.</summary>
public sealed class GetShopCouriersQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IUserProfileRepository profiles,
    CourierDirectory couriers)
    : IRequestHandler<GetShopCouriersQuery, Result<IReadOnlyList<CourierOptionDto>>>
{
    /// <summary>The seller themselves first, then the couriers in the shop's country.</summary>
    /// <returns>The list; or <see cref="VendorErrors.NotFound"/> when the caller has no shop.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<IReadOnlyList<CourierOptionDto>>> Handle(GetShopCouriersQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var names = await profiles.GetFullNamesAsync([userId], ct);
        var options = new List<CourierOptionDto> { new(userId, names.GetValueOrDefault(userId, "Me"), IsMe: true) };

        var people = await couriers.ListAsync(vendor.Country, ct);
        options.AddRange(people.Where(p => p.Id != userId)
                               .Select(p => new CourierOptionDto(p.Id, p.GetFullName(), false)));

        return Result<IReadOnlyList<CourierOptionDto>>.Success(options);
    }
}
