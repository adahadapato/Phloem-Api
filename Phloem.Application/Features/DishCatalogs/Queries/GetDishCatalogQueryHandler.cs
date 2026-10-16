using MediatR;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Queries;

public sealed class GetDishCatalogQueryHandler(IDishCatalogRepository catalog)
    : IRequestHandler<GetDishCatalogQuery, Result<IReadOnlyList<DishCatalogItemDto>>>
{
    public async Task<Result<IReadOnlyList<DishCatalogItemDto>>> Handle(GetDishCatalogQuery query, CancellationToken ct)
    {
        var approved = await catalog.GetApprovedAsync(ct);   // includes PriceRanges

        IReadOnlyList<DishCatalogItemDto> items = approved
            .Select(item => DishCatalogItemDto.From(item, query.Country))
            .ToList();

        return Result<IReadOnlyList<DishCatalogItemDto>>.Success(items);
    }
}