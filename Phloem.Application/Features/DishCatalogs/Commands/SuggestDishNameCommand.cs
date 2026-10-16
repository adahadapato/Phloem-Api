using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

/// <summary>Dish editor → "Can't find your dish?". Waits for an admin. Returns the suggestion's Id.</summary>
public sealed record SuggestDishNameCommand(string Name, DishCategory Category, CatalogGroup Group)
    : IRequest<Result<Guid>>;