using Phloem.Domain.Enums;
using Phloem.Application.Features.Moderation.Dtos;
using MediatR;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Queries;

public sealed class GetAdminDishSuggestionsQueryHandler(
    IDishCatalogRepository catalog,
    IUserProfileRepository profiles,
    IVendorRepository vendors,
    IModerationCheckRepository checks)
    : IRequestHandler<GetAdminDishSuggestionsQuery, Result<IReadOnlyList<AdminDishSuggestionDto>>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<IReadOnlyList<AdminDishSuggestionDto>>> Handle(GetAdminDishSuggestionsQuery query, CancellationToken ct)
    {
        var suggestions = await catalog.GetSuggestionsAsync(ct);

        // Names for the whole list: one query each, not one per row
        var userIds = suggestions
            .Where(s => s.SuggestedByUserId is not null)
            .Select(s => s.SuggestedByUserId!.Value)
            .Distinct()
            .ToList();

        var people = await profiles.GetFullNamesAsync(userIds, ct);
        var shops = await vendors.GetNamesByOwnerIdsAsync(userIds, ct);
        var advice = await checks.GetLatestAsync(ModerationKind.DishNameSuggestion,
                                                 suggestions.Select(s => s.Id).ToList(), ct);

        IReadOnlyList<AdminDishSuggestionDto> items = suggestions
            .Select(s =>
            {
                var userId = s.SuggestedByUserId;
                var person = userId is null ? UnknownUser : people.GetValueOrDefault(userId.Value, UnknownUser);
                var shop = userId is null ? null : shops.GetValueOrDefault(userId.Value);
                return AdminDishSuggestionDto.From(s, person, shop,
                    advice.TryGetValue(s.Id, out var a) ? ModerationAdviceDto.From(a) : null);
            })
            .ToList();

        return Result<IReadOnlyList<AdminDishSuggestionDto>>.Success(items);
    }
}