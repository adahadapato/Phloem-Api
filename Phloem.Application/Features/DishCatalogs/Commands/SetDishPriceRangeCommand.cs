using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

/// <summary>
/// Admin sets / changes the allowed price range (inclusive) for one dish name in one country.
/// Dishes now outside it become unsellable and their sellers are emailed.
/// Returns how many dishes were newly flagged (so the admin sees the effect).
/// </summary>
[InvalidatesCache(CacheTags.DishCatalog, CacheTags.Dishes)]
public sealed record SetDishPriceRangeCommand(Guid DishCatalogId, Country Country, decimal MinPrice, decimal MaxPrice)
    : IRequest<Result<int>>;