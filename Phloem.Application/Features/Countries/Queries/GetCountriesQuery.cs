using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Countries.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Countries.Queries;

/// <summary>
/// Every country Phloem works in, for the country pickers (app and website). Public.
/// Cached; cleared when an admin changes shipping routes (which decide "can send parcels").
/// </summary>
[Cacheable(CacheTags.ShipmentRoutes, Minutes = 60)]
public sealed record GetCountriesQuery : IRequest<Result<IReadOnlyList<CountryDto>>>;
