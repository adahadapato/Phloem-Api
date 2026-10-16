using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

/// <summary>
/// Admin approves a suggested name. Sellers can only use it once a price range is set for
/// their country (SetDishPriceRangeCommand), so the admin normally does both together.
/// </summary>
[InvalidatesCache(CacheTags.DishCatalog)]
public sealed record ApproveDishNameCommand(Guid DishCatalogId) : IRequest<Result>;