using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>Dish editor → Delete. Soft delete: past orders keep pointing at it.</summary>
[InvalidatesCache(CacheTags.Dishes, CacheTags.Vendors)]
public sealed record DeleteDishCommand(Guid DishId) : IRequest<Result>;