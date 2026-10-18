using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

/// <summary>
/// Found only if its shop is live. A dish that's sold out or needs a new price IS shown, with CanBeOrdered = false,
/// so links shared earlier still open.
/// </summary>
public sealed class GetDishQueryHandler(IDishRepository dishes, IFileStorage storage)
    : IRequestHandler<GetDishQuery, Result<DishDetailsDto>>
{
    public async Task<Result<DishDetailsDto>> Handle(GetDishQuery query, CancellationToken ct)
    {
        // GetByIdAsync includes the photos and the shop; deleted dishes are filtered out
        var dish = await dishes.GetByIdAsync(query.DishId, ct);
        if (dish is null || !dish.Vendor.IsVisibleToBuyers) return DishErrors.NotFound;

        return DishDetailsDto.From(dish, storage);
    }
}
