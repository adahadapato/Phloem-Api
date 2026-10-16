using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

public sealed class SearchDishesQueryHandler(IDishRepository dishes, IFileStorage storage)
    : IRequestHandler<SearchDishesQuery, Result<PagedResult<DishCardDto>>>
{
    public async Task<Result<PagedResult<DishCardDto>>> Handle(SearchDishesQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };

        var paged = await dishes.SearchAsync(
            query.Country, query.Category, query.Search?.Trim(), query.VendorId, query.Sort, page, ct);

        var items = paged.Items.Select(d => DishCardDto.From(d, storage)).ToList();
        return new PagedResult<DishCardDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}