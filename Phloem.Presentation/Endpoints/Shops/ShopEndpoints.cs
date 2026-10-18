using MediatR;
using Microsoft.AspNetCore.Mvc;
using Phloem.Application.Features.DeliveryPricings.Commands;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Application.Features.DeliveryPricings.Queries;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Application.Features.Vendors.Queries;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Shops;

/// <summary>
/// <c>/api/shops</c>: a seller's own shop. "mine" = the shop owned by the signed-in user (found from the token,
/// never from the URL), so nobody can change another shop.
/// </summary>
/// <remarks>Signed-in only, not the Seller role: the role is given when an admin approves the shop, and an owner must
/// be able to set it up before that.</remarks>
public static class ShopEndpoints
{
    /// <summary>Maps the shop endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapShopEndpoints(this IEndpointRouteBuilder api)
    {
        var shops = api.MapGroup("/shops")
                       .WithTags("Shops")
                       .RequireAuthorization();

        // ───────────────────────────── 1. Open and view ─────────────────────────────

        shops.MapPost("/", async (OpenShopCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("OpenShop")
            .WithSummary("Open my shop (needs date of birth + home address, 18+); waits for admin approval")
            .Produces<Guid>()
            .ProducesValidationProblem()
            .WithSummary("Open my shop; waits for admin approval")
            .WithDescription("Before this, save your date of birth and home address with " +
                             "PUT /api/me/personal-details (you must be 18 or over). " +
                             "This body takes only the shop's own details.")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        shops.MapGet("/mine", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyShopQuery(), ct)).ToHttp())
            .WithName("GetMyShop")
            .WithSummary("My shop: details, status, pictures, documents and what's still needed for the ✓ badge")
            .Produces<MyShopDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        shops.MapPut("/mine", async (UpdateShopDetailsCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("UpdateMyShop")
            .WithSummary("Change my shop's name, business email and phone")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        // ───────────────────────────── 2. Pictures ─────────────────────────────

        shops.MapPut("/mine/logo", async (IFormFile logo, ISender sender, CancellationToken ct) =>
        {
            await using var content = logo.OpenReadStream();
            return (await sender.Send(new ChangeShopImageCommand(ShopImage.Logo, content, logo.Length), ct)).ToHttp();
        })
            .WithName("ChangeMyShopLogo")
            .WithSummary("Upload my shop's logo (round picture on shop cards); returns its URL")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<string>()
            .ProducesValidationProblem()
            .DisableAntiforgery();

        shops.MapDelete("/mine/logo", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new RemoveShopImageCommand(ShopImage.Logo), ct)).ToHttp())
            .WithName("RemoveMyShopLogo")
            .WithSummary("Remove my shop's logo")
            .Produces(StatusCodes.Status204NoContent);

        shops.MapPut("/mine/cover-photo", async (IFormFile coverPhoto, ISender sender, CancellationToken ct) =>
        {
            await using var content = coverPhoto.OpenReadStream();
            return (await sender.Send(new ChangeShopImageCommand(ShopImage.CoverPhoto, content, coverPhoto.Length),
                                      ct)).ToHttp();
        })
            .WithName("ChangeMyShopCoverPhoto")
            .WithSummary("Upload my shop's cover photo (banner on the shop page); returns its URL")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<string>()
            .ProducesValidationProblem()
            .DisableAntiforgery();

        shops.MapDelete("/mine/cover-photo", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new RemoveShopImageCommand(ShopImage.CoverPhoto), ct)).ToHttp())
            .WithName("RemoveMyShopCoverPhoto")
            .WithSummary("Remove my shop's cover photo")
            .Produces(StatusCodes.Status204NoContent);

        // ───────────────────────────── 3. Documents ─────────────────────────────

        shops.MapPost("/mine/documents", async ([FromForm] VendorDocumentType type, [FromForm] DateOnly? expiresOn,
                                         IFormFile file, ISender sender, CancellationToken ct) =>
        {
            await using var content = file.OpenReadStream();
            var command = new UploadShopDocumentCommand(type, content, file.FileName, file.Length, expiresOn);
            return (await sender.Send(command, ct)).ToHttp();
        })
            .WithName("UploadMyShopDocument")
            .WithSummary("Upload a document for review (PDF, JPEG or PNG, max 10 MB); replaces the same type")
            .Produces<Guid>()
            .ProducesValidationProblem()
            .DisableAntiforgery();


        // ───────────────────────────── 4. Delivery fee ─────────────────────────────

        shops.MapGet("/mine/delivery-fee", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyDeliveryFeeQuery(), ct)).ToHttp())
            .WithName("GetMyDeliveryFee")
            .WithSummary("My delivery fee and the range allowed in my country")
            .Produces<MyDeliveryFeeDto>();

        shops.MapPut("/mine/delivery-fee", async (SetMyDeliveryFeeCommand command, ISender sender,
                                                  CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("SetMyDeliveryFee")
            .WithSummary("Set my delivery fee (must be inside my country's range)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return api;
    }
}