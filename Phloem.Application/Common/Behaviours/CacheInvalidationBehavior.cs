using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Results;
using System.Reflection;

namespace Phloem.Application.Common.Behaviors;

/// <summary>
/// After a command marked [InvalidatesCache] SUCCEEDS, removes every cached entry with its tags.
/// If the command fails, nothing changed, so nothing is removed.
/// </summary>
public sealed class CacheInvalidationBehavior<TRequest, TResponse>(HybridCache cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly InvalidatesCacheAttribute? Settings =
        typeof(TRequest).GetCustomAttribute<InvalidatesCacheAttribute>();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (Settings is null || response is Result { IsFailure: true })
            return response;

        foreach (var tag in Settings.Tags)
            await cache.RemoveByTagAsync(tag, cancellationToken);

        return response;
    }
}