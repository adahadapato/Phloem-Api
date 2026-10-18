using MediatR;
using Phloem.Application.Features.Dishes.Commands;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Application.Features.Dishes.Queries;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Dishes;

/// <summary>
/// <c>/api/shops/mine/dishes</c>: the seller's own menu. "mine" comes from the token, never the URL, so nobody can
/// change another shop's dishes. Dish names and their price ranges are in <c>DishCatalogEndpoints</c>.
/// </summary>
/// <remarks>Signed-in only, not the Seller role: a shop waiting for approval can prepare its menu. The Domain stops
/// blocked shops and checks every price against the admin's range.</remarks>
public static class DishEndpoints
{
    /// <summary>Maps the dish endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapDishEndpoints(this IEndpointRouteBuilder api)
    {
        var dishes = api.MapGroup("/shops/mine/dishes")
                        .WithTags("My dishes")
                        .RequireAuthorization();

        // ───────────────────────────── 1. My dishes ─────────────────────────────

        dishes.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyDishesQuery(), ct)).ToHttp())
            .WithName("GetMyDishes")
            .WithSummary("My dishes (those needing a new price first) and the \"needs a price update\" count")
            .Produces<MyDishesDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        dishes.MapPost("/", async (CreateDishCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("CreateMyDish")
            .WithSummary("Add a dish (name from the catalog, price inside its range); returns its Id")
            .WithDescription("Pick the name with GET /api/dish-catalog first. " +
                             "Add photos afterwards with POST /api/shops/mine/dishes/{dishId}/photos.")
            .Produces<Guid>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        dishes.MapPut("/{dishId:guid}", async (Guid dishId, DishRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new UpdateDishCommand(dishId, body.DishCatalogId, body.Detail, body.Price,
                                                         body.Description, body.IsAvailable), ct)).ToHttp())
            .WithName("UpdateMyDish")
            .WithSummary("Change a dish (name, detail, price, description, available)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        dishes.MapPut("/{dishId:guid}/price", async (Guid dishId, PriceRequest body, ISender sender,
                                                     CancellationToken ct) =>
                (await sender.Send(new ChangeDishPriceCommand(dishId, body.Price), ct)).ToHttp())
            .WithName("ChangeMyDishPrice")
            .WithSummary("Set a dish's NEW price (the dashboard +/− buttons)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        dishes.MapDelete("/{dishId:guid}", async (Guid dishId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new DeleteDishCommand(dishId), ct)).ToHttp())
            .WithName("DeleteMyDish")
            .WithSummary("Delete a dish (past orders still show it)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────────────────────── 2. Photos ─────────────────────────────

        dishes.MapPost("/{dishId:guid}/photos", async (Guid dishId, IFormFile photo, ISender sender,
                                                       CancellationToken ct) =>
        {
            await using var content = photo.OpenReadStream();
            return (await sender.Send(new AddDishPhotoCommand(dishId, content, photo.Length), ct)).ToHttp();
        })
            .WithName("AddMyDishPhoto")
            .WithSummary("Add a photo (up to 5 per dish, max 10 MB); returns the photo")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<DishPhotoDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery();

        dishes.MapDelete("/{dishId:guid}/photos/{photoId:guid}", async (Guid dishId, Guid photoId, ISender sender,
                                                                       CancellationToken ct) =>
                (await sender.Send(new RemoveDishPhotoCommand(dishId, photoId), ct)).ToHttp())
            .WithName("RemoveMyDishPhoto")
            .WithSummary("Remove a photo from a dish")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }

    /// <summary>Body of "change a dish" (the dish Id comes from the URL).</summary>
    public sealed record DishRequest(Guid DishCatalogId, string? Detail, decimal Price, string? Description,
                                     bool IsAvailable);

    /// <summary>Body of "set the price": the NEW price, not "+1".</summary>
    public sealed record PriceRequest(decimal Price);
}
