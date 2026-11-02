using MediatR;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Queries;

/// <summary>Handles <see cref="GetAdminDeliveryPricingsQuery"/>.</summary>
public sealed class GetAdminDeliveryPricingsQueryHandler(ICurrentUser currentUser,
                                                         IDeliveryPricingRepository pricings)
    : IRequestHandler<GetAdminDeliveryPricingsQuery, Result<IReadOnlyList<DeliveryPricingDto>>>
{
    /// <summary>Loads the pricing of every country the person covers, ordered by country.</summary>
    public async Task<Result<IReadOnlyList<DeliveryPricingDto>>> Handle(
        GetAdminDeliveryPricingsQuery query, CancellationToken ct)
    {
        var all = await pricings.GetAllAsync(ct);

        IReadOnlyList<DeliveryPricingDto> items = all
            .Where(p => currentUser.CoversCountry(p.Country))
            .OrderBy(p => p.Country)
            .Select(DeliveryPricingDto.From)
            .ToList();

        return Result<IReadOnlyList<DeliveryPricingDto>>.Success(items);
    }
}