using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>Home → "Top shops": the best-rated approved shops, optionally in one country.</summary>
[Cacheable(CacheTags.Vendors, Minutes = 5)]
public sealed record GetTopShopsQuery(Country? Country, int Count = GetTopShopsQuery.DefaultCount)
    : IRequest<Result<IReadOnlyList<ShopListItemDto>>>
{
    /// <summary>How many shops the Home screen shows when the app doesn't say.</summary>
    public const int DefaultCount = 8;

    /// <summary>The most the app may ask for at once.</summary>
    public const int MaxCount = 20;
}
