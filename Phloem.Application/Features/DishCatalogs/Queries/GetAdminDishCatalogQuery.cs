using MediatR;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Queries;

/// <summary>Admin → dish names and their price ranges. Not cached (admins need live data).</summary>
public sealed record GetAdminDishCatalogQuery : IRequest<Result<IReadOnlyList<AdminDishCatalogDto>>>;