using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>Dish editor → Save (existing dish). DishId comes from the URL.</summary>
[InvalidatesCache(CacheTags.Dishes)]
public sealed record UpdateDishCommand(
    Guid DishId,
    Guid DishCatalogId,
    string? Detail,
    decimal Price,
    string? Description,
    bool IsAvailable) : IRequest<Result>;