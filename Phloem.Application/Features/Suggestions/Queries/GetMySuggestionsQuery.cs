using MediatR;
using Phloem.Application.Features.Suggestions.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Suggestions.Queries;

/// <summary>
/// Home for a signed-in customer: "Order again" (dishes they've bought, on sale now) and "For you" (new dishes picked
/// from what they buy, each with its reason). Only dishes that can be ordered now in <paramref name="Country"/>.
/// Never cached (it changes with every order).
/// </summary>
/// <param name="Country">The country they're shopping in (Home's country).</param>
/// <param name="Take">How many dishes in each row (1–20).</param>
public sealed record GetMySuggestionsQuery(Country Country, int Take = GetMySuggestionsQuery.DefaultTake)
    : IRequest<Result<SuggestionsDto>>
{
    /// <summary>How many per row when the caller doesn't say.</summary>
    public const int DefaultTake = 8;

    /// <summary>Most per row.</summary>
    public const int MaxTake = 20;
}
