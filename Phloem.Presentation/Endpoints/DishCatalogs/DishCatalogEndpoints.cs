using MediatR;
using Phloem.Application.Features.DishCatalogs.Commands;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Application.Features.DishCatalogs.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.DishCatalogs;

/// <summary>
/// The dish catalog: the admin-controlled list of dish names sellers can sell, each with a price range per country.
/// Two groups:
/// <list type="number">
///   <item><description><b>Sellers</b> (<c>/api/dish-catalog</c>): the name picker and "Can't find your dish?".</description></item>
///   <item><description><b>Admin</b> (<c>/api/admin/dish-catalog</c>): review suggested names and set price ranges.
///   Admin role only.</description></item>
/// </list>
/// </summary>
public static class DishCatalogEndpoints
{
    /// <summary>Maps the dish catalog endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapDishCatalogEndpoints(this IEndpointRouteBuilder api)
    {
        MapSellers(api);
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. Sellers ═════════════════════════════

    private static void MapSellers(IEndpointRouteBuilder api)
    {
        var catalog = api.MapGroup("/dish-catalog")
                         .WithTags("Dish catalog")
                         .RequireAuthorization();

        catalog.MapGet("/", async ([AsParameters] GetDishCatalogQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetDishCatalog")
            .WithSummary("Dish names for the picker, with the price range in a country (use your shop's country)")
            .WithDescription("Names with hasPriceRange = false can't be sold yet: an admin hasn't set their range.")
            .Produces<IReadOnlyList<DishCatalogItemDto>>()
            .ProducesValidationProblem();

        catalog.MapPost("/suggestions", async (SuggestDishNameCommand command, ISender sender,
                                               CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("SuggestDishName")
            .WithSummary("\"Can't find your dish?\": suggest a name for an admin to approve; returns its Id")
            .Produces<Guid>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ═════════════════════════════ B. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/dish-catalog")
                       .WithTags("Admin: Dish catalog")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // ───────────────────────────── 1. Lists ─────────────────────────────

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminDishCatalogQuery(), ct)).ToHttp())
            .WithName("GetAdminDishCatalog")
            .WithSummary("All dish names with their price range in every country")
            .Produces<IReadOnlyList<AdminDishCatalogDto>>();

        admin.MapGet("/suggestions", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminDishSuggestionsQuery(), ct)).ToHttp())
            .WithName("GetDishNameSuggestions")
            .WithSummary("Names sellers suggested, waiting for review (oldest first)")
            .Produces<IReadOnlyList<AdminDishSuggestionDto>>();

        // ───────────────────────────── 2. Decisions ─────────────────────────────

        admin.MapPost("/{dishCatalogId:guid}/approve", async (Guid dishCatalogId, ISender sender,
                                                              CancellationToken ct) =>
                (await sender.Send(new ApproveDishNameCommand(dishCatalogId), ct)).ToHttp())
            .WithName("ApproveDishName")
            .WithSummary("Approve a suggested name (sellers can use it once it has a price range)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPost("/{dishCatalogId:guid}/reject", async (Guid dishCatalogId, ISender sender,
                                                             CancellationToken ct) =>
                (await sender.Send(new RejectDishNameCommand(dishCatalogId), ct)).ToHttp())
            .WithName("RejectDishName")
            .WithSummary("Reject a suggested name")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPut("/{dishCatalogId:guid}/price-ranges/{country}",
                async (Guid dishCatalogId, Country country, PriceRangeRequest body, ISender sender,
                       CancellationToken ct) =>
                    (await sender.Send(new SetDishPriceRangeCommand(dishCatalogId, country, body.MinPrice,
                                                                    body.MaxPrice), ct)).ToHttp())
            .WithName("SetDishPriceRange")
            .WithSummary("Set a name's price range in one country; returns how many dishes now need a new price")
            .WithDescription("Sellers whose dishes fall outside the new range are emailed and must change the price.")
            .Produces<int>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "set a price range" (the name and country come from the URL). Both ends are allowed.</summary>
    public sealed record PriceRangeRequest(decimal MinPrice, decimal MaxPrice);
}
