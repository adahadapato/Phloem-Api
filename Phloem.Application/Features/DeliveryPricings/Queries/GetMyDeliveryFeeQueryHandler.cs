using MediatR;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Queries;

/// <summary>Handles <see cref="GetMyDeliveryFeeQuery"/>.</summary>
public sealed class GetMyDeliveryFeeQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDeliveryPricingRepository pricings)
    : IRequestHandler<GetMyDeliveryFeeQuery, Result<MyDeliveryFeeDto>>
{
    /// <summary>
    /// Returns the seller's fee and range; <see cref="VendorErrors.NotFound"/> if they have no shop;
    /// <see cref="DeliveryPricingErrors.NotSet"/> if their country has no pricing yet.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<MyDeliveryFeeDto>> Handle(GetMyDeliveryFeeQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var pricing = await pricings.GetByCountryAsync(vendor.Country, ct);
        if (pricing is null) return DeliveryPricingErrors.NotSet;

        return new MyDeliveryFeeDto(
            CurrentFee: vendor.DeliveryFee ?? pricing.MinShopFee,
            UsesCountryMinimum: vendor.DeliveryFee is null,
            MinAllowed: pricing.MinShopFee,
            MaxAllowed: pricing.MaxShopFee,
            Currency: pricing.Currency);
    }
}