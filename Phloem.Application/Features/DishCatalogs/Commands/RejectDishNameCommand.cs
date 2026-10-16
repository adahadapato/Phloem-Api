using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

/// <summary>Admin rejects a suggested name. It can be suggested again later.</summary>
public sealed record RejectDishNameCommand(Guid DishCatalogId) : IRequest<Result>;