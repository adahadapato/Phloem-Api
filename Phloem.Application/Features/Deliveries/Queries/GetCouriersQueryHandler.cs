using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Deliveries.Queries;

/// <summary>Handles <see cref="GetCouriersQuery"/>.</summary>
public sealed class GetCouriersQueryHandler(CourierDirectory couriers)
    : IRequestHandler<GetCouriersQuery, Result<IReadOnlyList<CourierOptionDto>>>
{
    /// <summary>Couriers in the country (not suspended), by name; empty when there are none yet.</summary>
    public async Task<Result<IReadOnlyList<CourierOptionDto>>> Handle(GetCouriersQuery query, CancellationToken ct)
    {
        var people = await couriers.ListAsync(query.Country, ct);
        IReadOnlyList<CourierOptionDto> options = people.Select(p => new CourierOptionDto(p.Id, p.GetFullName(), false))
                                                        .ToList();
        return Result<IReadOnlyList<CourierOptionDto>>.Success(options);
    }
}
