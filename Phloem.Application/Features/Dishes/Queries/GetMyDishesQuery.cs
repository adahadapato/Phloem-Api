using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

/// <summary>Seller dashboard → "My dishes" + needs-price-update banner. Not cached (seller's own data).</summary>
public sealed record GetMyDishesQuery : IRequest<Result<MyDishesDto>>;