using MediatR;
using Microsoft.AspNetCore.Mvc;
using Phloem.Application.Features.DeliveryPricings.Commands;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Application.Features.DeliveryPricings.Queries;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Application.Features.Vendors.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Shops;

/// <summary>
/// Every shop endpoint, in two groups:
/// <list type="number">
///   <item><description><b>My shop</b> (<c>/api/shops</c>): the signed-in user's own shop. "mine" is found from the
///   token, never from the URL, so nobody can change another shop.</description></item>
///   <item><description><b>Admin</b> (<c>/api/admin/shops</c>): review, approve, verify and block shops.
///   Admin role only.</description></item>
/// </list>
/// </summary>
/// <remarks>"My shop" needs sign-in, not the Seller role: the role is given when an admin approves the shop, and an
/// owner must be able to set it up before that.</remarks>
public static class ShopEndpoints
{
    /// <summary>Maps the shop endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapShopEndpoints(this IEndpointRouteBuilder api)
    {
        MapMyShop(api);
        MapAdmin(api);
        return api;
    }

    // ═════════════════════════════ A. My shop ═════════════════════════════

    private static void MapMyShop(IEndpointRouteBuilder api)
    {
        var shops = api.MapGroup("/shops")
                       .WithTags("Shops")
                       .RequireAuthorization();

        // ───────────────────────────── 1. Open and view ─────────────────────────────

        shops.MapPost("/", async (OpenShopCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("OpenShop")
            .WithSummary("Open my shop; waits for admin approval")
            .WithDescription("Before this, save your date of birth and home address with " +
                             "PUT /api/me/personal-details (you must be 18 or over). " +
                             "This body takes only the shop's own details.")
            .Produces<Guid>()
            .ProducesValidationProblem()
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
    }

    // ═════════════════════════════ B. Admin ═════════════════════════════

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/shops")
                       .WithTags("Admin: Shops")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        // ───────────────────────────── 1. Lists ─────────────────────────────

        admin.MapGet("/", async ([AsParameters] GetAdminShopsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetAdminShops")
            .WithSummary("All shops, filtered by status / country / name, with owner names")
            .Produces<PagedResult<AdminShopDto>>();

        admin.MapGet("/pending", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetPendingShopsQuery(), ct)).ToHttp())
            .WithName("GetPendingShops")
            .WithSummary("Shops waiting for approval, oldest first")
            .Produces<IReadOnlyList<AdminShopDto>>();

        admin.MapGet("/stats", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminShopStatsQuery(), ct)).ToHttp())
            .WithName("GetAdminShopStats")
            .WithSummary("Number of shops in each status")
            .Produces<AdminShopStatsDto>();

        // ───────────────────────────── 2. One shop ─────────────────────────────

        admin.MapGet("/{shopId:guid}", async (Guid shopId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminShopQuery(shopId), ct)).ToHttp())
            .WithName("GetAdminShop")
            .WithSummary("One shop with its documents and what's missing for the ✓ badge")
            .Produces<AdminShopDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapGet("/{shopId:guid}/documents/{documentId:guid}/link",
                async (Guid shopId, Guid documentId, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new GetShopDocumentLinkQuery(shopId, documentId), ct)).ToHttp())
            .WithName("GetShopDocumentLink")
            .WithSummary("A 10-minute link to open a private document in the browser")
            .Produces<string>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────────────────────── 3. Decisions ─────────────────────────────

        admin.MapPost("/{shopId:guid}/approve", async (Guid shopId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new ApproveShopCommand(shopId), ct)).ToHttp())
            .WithName("ApproveShop")
            .WithSummary("Approve: the shop goes live and its owner becomes a Seller")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPost("/{shopId:guid}/reject", async (Guid shopId, ReasonRequest body, ISender sender,
                                                      CancellationToken ct) =>
                (await sender.Send(new RejectShopCommand(shopId, body.Reason), ct)).ToHttp())
            .WithName("RejectShop")
            .WithSummary("Reject with a reason the seller sees and can fix")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPost("/{shopId:guid}/documents/{documentId:guid}/review",
                async (Guid shopId, Guid documentId, ReviewRequest body, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new ReviewShopDocumentCommand(shopId, documentId, body.Accept, body.Reason),
                                       ct)).ToHttp())
            .WithName("ReviewShopDocument")
            .WithSummary("Accept or reject a document (a reason is required to reject)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPut("/{shopId:guid}/verified", async (Guid shopId, VerifiedRequest body, ISender sender,
                                                       CancellationToken ct) =>
                (await sender.Send(new SetShopVerifiedCommand(shopId, body.Verified), ct)).ToHttp())
            .WithName("SetShopVerified")
            .WithSummary("Give or remove the ✓ badge (giving it needs the required documents accepted)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        admin.MapPut("/{shopId:guid}/blocked", async (Guid shopId, BlockedRequest body, ISender sender,
                                                      CancellationToken ct) =>
                (await sender.Send(new SetShopBlockedCommand(shopId, body.Blocked), ct)).ToHttp())
            .WithName("SetShopBlocked")
            .WithSummary("Block (hide, Seller role removed) or unblock a shop")
            .Produces(StatusCodes.Status204NoContent);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "reject a shop": why.</summary>
    public sealed record ReasonRequest(string Reason);

    /// <summary>Body of "review a document". Reason is required when rejecting.</summary>
    public sealed record ReviewRequest(bool Accept, string? Reason);

    /// <summary>Body of "set verified".</summary>
    public sealed record VerifiedRequest(bool Verified);

    /// <summary>Body of "set blocked".</summary>
    public sealed record BlockedRequest(bool Blocked);
}
