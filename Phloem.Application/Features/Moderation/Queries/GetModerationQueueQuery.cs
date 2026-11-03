using MediatR;
using Phloem.Application.Common.Authorization;
using Phloem.Application.Features.Moderation.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Moderation.Queries;

/// <summary>Admin → Moderation: what needs a person, oldest first, in the person's countries. Not cached.</summary>
/// <param name="Kind">Only dishes or only shop names; null = both.</param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Items per page.</param>
public sealed record GetModerationQueueQuery(ModerationKind? Kind, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ModerationItemDto>>>;

/// <summary>Handles <see cref="GetModerationQueueQuery"/>.</summary>
public sealed class GetModerationQueueQueryHandler(
    ICurrentUser currentUser,
    IModerationCheckRepository checks,
    IDishRepository dishes,
    IVendorRepository vendors,
    IModerationSettingsProvider settings)
    : IRequestHandler<GetModerationQueueQuery, Result<PagedResult<ModerationItemDto>>>
{
    /// <summary>One page of the queue, with each dish's or shop's name.</summary>
    public async Task<Result<PagedResult<ModerationItemDto>>> Handle(GetModerationQueueQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await checks.GetWaitingForPersonAsync(query.Kind, currentUser.CountryLimit(), page, ct);

        var dishIds = paged.Items.Where(c => c.Kind == ModerationKind.Dish).Select(c => c.TargetId).ToList();
        var dishById = (await dishes.GetByIdsAsync(dishIds, ct)).ToDictionary(d => d.Id);

        var items = new List<ModerationItemDto>();
        foreach (var c in paged.Items)
        {
            string title; Guid? shopId; string? shopName; bool live;
            if (c.Kind == ModerationKind.Dish && dishById.TryGetValue(c.TargetId, out var dish))
            {
                title = dish.Name; shopId = dish.VendorId; shopName = dish.Vendor?.Name;
                live = dish.ReviewStatus == ContentReviewStatus.Approved;
            }
            else if (c.Kind == ModerationKind.ShopName && await vendors.GetByIdAsync(c.TargetId, ct) is { } shop)
            {
                title = shop.Name; shopId = shop.Id; shopName = shop.Name; live = shop.IsVisibleToBuyers;
            }
            else
            {
                title = c.Text.Split('\n')[0]; shopId = null; shopName = null; live = false;
            }

            var why = c.AiVerdict switch
            {
                null => c.AiReason ?? "The AI couldn't check it.",
                ModerationVerdict.Ok => settings.HoldAll ? "\"Hold everything for a person\" is on." : "Sent for a second look.",
                ModerationVerdict.NotAllowed => "The AI thinks this isn't allowed.",
                _ => "The AI wasn't sure."
            };

            items.Add(new ModerationItemDto(c.Id, c.Kind, title, shopId, shopName, c.Country, c.Text, c.AiVerdict,
                                            c.AiVerdict is null ? null : c.AiReason, c.AiLabel, why, live,
                                            c.CreatedAt));
        }
        return new PagedResult<ModerationItemDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}

/// <summary>How many items wait for a person in the person's countries (the number next to the menu item).</summary>
public sealed record GetModerationCountQuery : IRequest<Result<int>>;

/// <summary>Handles <see cref="GetModerationCountQuery"/>.</summary>
public sealed class GetModerationCountQueryHandler(ICurrentUser currentUser, IModerationCheckRepository checks)
    : IRequestHandler<GetModerationCountQuery, Result<int>>
{
    /// <summary>The total waiting.</summary>
    public async Task<Result<int>> Handle(GetModerationCountQuery query, CancellationToken ct) =>
        (await checks.CountWaitingForPersonAsync(currentUser.CountryLimit(), ct)).Values.Sum();
}
