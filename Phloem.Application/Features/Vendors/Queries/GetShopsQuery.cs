using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>
/// "Meet local shops": approved shops only, optionally in one country and/or matching a name.
/// Cached briefly; cleared when a shop is approved, blocked or changes its details or pictures.
/// </summary>
[Cacheable(CacheTags.Vendors, Minutes = 5)]
public sealed record GetShopsQuery(
    Country? Country,
    string? Search,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IRequest<Result<PagedResult<ShopListItemDto>>>;
