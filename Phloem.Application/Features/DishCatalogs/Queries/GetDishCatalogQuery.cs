using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Queries;

/// <summary>
/// Add / Edit dish → name picker. Country = the seller's shop country (ranges differ per country).
/// Cached for an hour; cleared when a name is approved or a range changes.
/// </summary>
[Cacheable(CacheTags.DishCatalog, Minutes = 60)]
public sealed record GetDishCatalogQuery(Country Country) : IRequest<Result<IReadOnlyList<DishCatalogItemDto>>>;