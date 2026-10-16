using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>Dish editor → Save (new dish). Returns the new dish's Id (photo is added next).</summary>
[InvalidatesCache(CacheTags.Dishes, CacheTags.Vendors)]
public sealed record CreateDishCommand(
    Guid DishCatalogId,
    string? Detail,
    decimal Price,
    string? Description,
    bool IsAvailable) : IRequest<Result<Guid>>;