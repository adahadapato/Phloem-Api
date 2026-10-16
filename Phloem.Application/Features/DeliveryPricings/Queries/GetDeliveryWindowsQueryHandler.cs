using MediatR;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Queries;

/// <summary>Handles <see cref="GetDeliveryWindowsQuery"/>.</summary>
public sealed class GetDeliveryWindowsQueryHandler(IDeliveryPricingRepository pricings)
    : IRequestHandler<GetDeliveryWindowsQuery, Result<IReadOnlyList<DeliveryWindowOptionDto>>>
{
    /// <summary>
    /// Returns the offered windows in window order, or <see cref="DeliveryPricingErrors.NotSet"/>
    /// if delivery isn't set up in that country yet.
    /// </summary>
    public async Task<Result<IReadOnlyList<DeliveryWindowOptionDto>>> Handle(
        GetDeliveryWindowsQuery query, CancellationToken ct)
    {
        var pricing = await pricings.GetByCountryAsync(query.Country, ct);
        if (pricing is null) return DeliveryPricingErrors.NotSet;

        IReadOnlyList<DeliveryWindowOptionDto> options = pricing.WindowCharges
            .OrderBy(c => c.Window)
            .Select(c => new DeliveryWindowOptionDto(c.Window, c.ExtraCharge, pricing.Currency))
            .ToList();

        return Result<IReadOnlyList<DeliveryWindowOptionDto>>.Success(options);
    }
}