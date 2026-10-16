using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>Seller dashboard +/− buttons: the app sends the NEW price, not "+1".</summary>
[InvalidatesCache(CacheTags.Dishes)]
public sealed record ChangeDishPriceCommand(Guid DishId, decimal Price) : IRequest<Result>;