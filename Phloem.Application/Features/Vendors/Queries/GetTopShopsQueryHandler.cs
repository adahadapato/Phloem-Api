using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetTopShopsQueryHandler(
    IVendorRepository vendors,
    IDishRepository dishes,
    IFileStorage storage)
    : IRequestHandler<GetTopShopsQuery, Result<IReadOnlyList<ShopListItemDto>>>
{
    public async Task<Result<IReadOnlyList<ShopListItemDto>>> Handle(GetTopShopsQuery query, CancellationToken ct)
    {
        var top = await vendors.GetTopRatedAsync(query.Country, query.Count, ct);   // approved only

        var counts = await dishes.CountByVendorsAsync(top.Select(v => v.Id).ToList(), ct);
        IReadOnlyList<ShopListItemDto> items = top
            .Select(v => ShopListItemDto.From(v, counts.GetValueOrDefault(v.Id), storage))
            .ToList();

        return Result<IReadOnlyList<ShopListItemDto>>.Success(items);
    }
}
