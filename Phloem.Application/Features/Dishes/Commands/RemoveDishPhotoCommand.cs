using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

/// <summary>Dish editor → Remove photo. The file is kept (past orders may show it).</summary>
[InvalidatesCache(CacheTags.Dishes)]
public sealed record RemoveDishPhotoCommand(Guid DishId, Guid PhotoId) : IRequest<Result>;