using MediatR;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Queries;

/// <summary>
/// Admin → "Dish name suggestions": names waiting for review, oldest first,
/// with who suggested them and from which shop. Not cached: admins need live data.
/// </summary>
public sealed record GetAdminDishSuggestionsQuery : IRequest<Result<IReadOnlyList<AdminDishSuggestionDto>>>;