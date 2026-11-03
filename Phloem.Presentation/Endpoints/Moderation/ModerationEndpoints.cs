using MediatR;
using Phloem.Application.Features.Moderation.Commands;
using Phloem.Application.Features.Moderation.Dtos;
using Phloem.Application.Features.Moderation.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authorization;

namespace Phloem.Presentation.Endpoints.Moderation;

/// <summary>
/// Admin → Moderation (<c>/api/admin/moderation</c>, permission <c>shops.manage</c>, the person's countries): dishes
/// and shop names the AI flagged or couldn't check. Approve (a dish becomes visible) or reject with a reason (the
/// seller sees it).
/// </summary>
public static class ModerationEndpoints
{
    /// <summary>Maps the moderation endpoints.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapModerationEndpoints(this IEndpointRouteBuilder api)
    {
        var moderation = api.MapGroup("/admin/moderation")
                            .WithTags("Admin: Moderation")
                            .RequirePermission(Permissions.Shops.Manage, CountryRule.RecordCountry);

        moderation.MapGet("/", async ([AsParameters] GetModerationQueueQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetModerationQueue")
            .WithSummary("What needs a person, oldest first (kind = Dish | ShopName)")
            .Produces<PagedResult<ModerationItemDto>>()
            .ProducesValidationProblem();

        moderation.MapGet("/count", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetModerationCountQuery(), ct)).ToHttp())
            .WithName("GetModerationCount")
            .WithSummary("How many items wait for a person (the number in the menu)")
            .Produces<int>();

        moderation.MapPost("/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
                (await sender.Send(new DecideModerationCommand(id, true, null), ct)).ToHttp())
            .WithName("ApproveModeration")
            .WithSummary("Fine: a dish becomes visible to buyers")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        moderation.MapPost("/{id:guid}/reject", async (Guid id, RejectRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new DecideModerationCommand(id, false, body.Reason), ct)).ToHttp())
            .WithName("RejectModeration")
            .WithSummary("Not allowed, with a reason (a dish stays hidden; the seller sees the reason)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return api;
    }

    /// <summary>Body of "reject".</summary>
    /// <param name="Reason">Why (for dishes, the seller sees it).</param>
    public sealed record RejectRequest(string Reason);
}
