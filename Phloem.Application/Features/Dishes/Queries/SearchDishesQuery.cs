using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

/// <summary>
/// Home + Food listing. Country / Category null = "All". VendorId = "View all" for one shop.
/// Cached briefly; cleared whenever a dish, price, shop status or price range changes.
/// </summary>
[Cacheable(CacheTags.Dishes, Minutes = 2)]
public sealed record SearchDishesQuery(
    Country? Country,
    DishCategory? Category,
    string? Search,
    Guid? VendorId,
    DishSort Sort = DishSort.Rating,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<DishCardDto>>>;