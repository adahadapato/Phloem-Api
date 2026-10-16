using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>
/// Dish editor → Take / Choose photo. Runs the strict dish-photo pipeline
/// (1200×900 + 400×300 thumbnail, WebP). Returns the photo so the app can show it at once.
/// </summary>
[InvalidatesCache(CacheTags.Dishes)]
public sealed record AddDishPhotoCommand(Guid DishId, Stream Content, long SizeBytes)
    : IRequest<Result<DishPhotoDto>>;