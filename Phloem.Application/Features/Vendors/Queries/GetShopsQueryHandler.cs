using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>One page of approved shops, then the dish counts in ONE extra query (not one per shop).</summary>
public sealed class GetShopsQueryHandler(
    IVendorRepository vendors,
    IDishRepository dishes,
    IFileStorage storage)
    : IRequestHandler<GetShopsQuery, Result<PagedResult<ShopListItemDto>>>
{
    public async Task<Result<PagedResult<ShopListItemDto>>> Handle(GetShopsQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await vendors.GetPagedAsync(status: null, query.Country, query.Search?.Trim(), page, ct,
                                                visibleToBuyersOnly: true);

        var counts = await dishes.CountByVendorsAsync(paged.Items.Select(v => v.Id).ToList(), ct);
        var items = paged.Items
            .Select(v => ShopListItemDto.From(v, counts.GetValueOrDefault(v.Id), storage))
            .ToList();

        return new PagedResult<ShopListItemDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
