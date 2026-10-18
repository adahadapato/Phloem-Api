using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

/// <summary>A dish's page, as buyers see it. Dishes of shops that aren't live are not found.</summary>
[Cacheable(CacheTags.Dishes, Minutes = 2)]
public sealed record GetDishQuery(Guid DishId) : IRequest<Result<DishDetailsDto>>;
