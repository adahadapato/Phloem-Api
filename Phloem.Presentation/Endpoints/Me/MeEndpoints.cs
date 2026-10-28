using MediatR;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Application.Features.Accounts.Queries;
using Phloem.Application.Features.LegalDocuments.Commands;
using Phloem.Application.Features.Suggestions.Dtos;
using Phloem.Application.Features.Suggestions.Queries;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Me;

/// <summary>
/// <c>/api/me</c>: the signed-in user's own account. Who "me" is comes from the access token (via <c>ICurrentUser</c>
/// in the handlers), never from the URL or body, so nobody can change someone else's account.
/// </summary>
/// <remarks>Every endpoint needs a valid access token (lock icon in Swagger).</remarks>
public static class MeEndpoints
{
    /// <summary>Maps the <c>/me</c> endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder api)
    {
        var me = api.MapGroup("/me")
                    .WithTags("Me")
                    .RequireAuthorization();

        // ───────────────────────────── 1. Profile ─────────────────────────────

        me.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyProfileQuery(), ct)).ToHttp())
            .WithName("GetMyProfile")
            .WithSummary("My profile: name, country, photo, addresses, notification switches")
            .Produces<ProfileDto>();

        me.MapPut("/", async (UpdateProfileCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("UpdateMyProfile")
            .WithSummary("Change my name, country and phone number")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        me.MapPut("/notifications", async (UpdateNotificationsCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("UpdateMyNotifications")
            .WithSummary("Turn my notification switches on or off")
            .Produces(StatusCodes.Status204NoContent);

        me.MapPut("/personal-details", async (UpdatePersonalDetailsCommand command, ISender sender,
                                      CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttp())
            .WithName("UpdateMyPersonalDetails")
            .WithSummary("Save my date of birth and home address (needed to open a shop or send a parcel)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        me.MapGet("/suggestions", async ([AsParameters] GetMySuggestionsQuery query, ISender sender,
                                          CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetMySuggestions")
            .WithSummary("Home: \"Order again\" and \"For you\" (new dishes picked from my orders, each with a reason)")
            .WithDescription("country = the country Home shows (only dishes that can be ordered there now). " +
                             "isPersonal = false when I have no orders yet: forYou is then \"Popular in …\".")
            .Produces<SuggestionsDto>()
            .ProducesValidationProblem();

        // Terms & Privacy: "We've updated our Terms" → Accept; "We've updated our Privacy Policy" → Got it
        me.MapPost("/terms/accept", async (VersionRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new AcceptTermsCommand(body.Version), ct)).ToHttp())
            .WithName("AcceptTerms")
            .WithSummary("Accept the latest Terms (the version shown); 409 Terms.Outdated if a newer one was published")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        me.MapPost("/privacy/seen", async (VersionRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new AcknowledgePrivacyCommand(body.Version), ct)).ToHttp())
            .WithName("AcknowledgePrivacy")
            .WithSummary("I've been shown this version of the Privacy Policy (\"Got it\")")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        // ───────────────────────────── 2. Photo ─────────────────────────────

        me.MapPut("/photo", async (IFormFile photo, ISender sender, CancellationToken ct) =>
        {
            await using var content = photo.OpenReadStream();
            return (await sender.Send(new ChangePhotoCommand(content, photo.Length), ct)).ToHttp();
        })
            .WithName("ChangeMyPhoto")
            .WithSummary("Upload a new profile photo (JPEG, PNG, WebP or HEIC); returns its URL")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<string>()
            .ProducesValidationProblem()
            .DisableAntiforgery();   // JWT API: no cookies, so no cross-site form risk

        me.MapDelete("/photo", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new RemovePhotoCommand(), ct)).ToHttp())
            .WithName("RemoveMyPhoto")
            .WithSummary("Remove my profile photo")
            .Produces(StatusCodes.Status204NoContent);

        // ───────────────────────────── 3. Addresses ─────────────────────────────

        me.MapPost("/addresses", async (AddAddressCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("AddMyAddress")
            .WithSummary("Add a delivery address; returns its Id")
            .Produces<Guid>()
            .ProducesValidationProblem();

        me.MapPut("/addresses/{addressId:guid}", async (Guid addressId, AddressRequest body, ISender sender,
                                                        CancellationToken ct) =>
                (await sender.Send(new UpdateAddressCommand(addressId, body.Label, body.FullAddress, body.Country,
                                                            body.RecipientName, body.PhoneNumber),
                                   ct)).ToHttp())
            .WithName("UpdateMyAddress")
            .WithSummary("Change one of my delivery addresses")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        me.MapDelete("/addresses/{addressId:guid}", async (Guid addressId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new RemoveAddressCommand(addressId), ct)).ToHttp())
            .WithName("RemoveMyAddress")
            .WithSummary("Remove one of my delivery addresses")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        me.MapPut("/addresses/{addressId:guid}/default", async (Guid addressId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new SetDefaultAddressCommand(addressId), ct)).ToHttp())
            .WithName("SetMyDefaultAddress")
            .WithSummary("Make one of my addresses the default")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // ───────────────────────────── 4. Password ─────────────────────────────

        me.MapPut("/password", async (ChangePasswordCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("ChangeMyPassword")
            .WithSummary("Change my password (signs out every device; sign in again afterwards)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        // ───────────────────────────── 5. Two-factor ─────────────────────────────

        me.MapGet("/two-factor", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyTwoFactorStatusQuery(), ct)).ToHttp())
            .WithName("GetMyTwoFactorStatus")
            .WithSummary("Is two-factor sign-in on, and how many recovery codes are left")
            .Produces<TwoFactorStatusDto>();

        me.MapGet("/two-factor/setup", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyTwoFactorSetupQuery(), ct)).ToHttp())
            .WithName("GetMyTwoFactorSetup")
            .WithSummary("The key and QR-code link for my authenticator app")
            .Produces<TwoFactorSetupDto>();

        me.MapPost("/two-factor/enable", async (EnableTwoFactorCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("EnableMyTwoFactor")
            .WithSummary("Turn two-factor on with a code from the app; returns recovery codes (show them once)")
            .Produces<IReadOnlyCollection<string>>()
            .ProducesValidationProblem();

        me.MapPost("/two-factor/disable", async (DisableTwoFactorCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("DisableMyTwoFactor")
            .WithSummary("Turn two-factor off (needs my password; not allowed for admins)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden);

        me.MapPost("/two-factor/recovery-codes", async (RegenerateRecoveryCodesCommand command, ISender sender,
                                                        CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .WithName("RegenerateMyRecoveryCodes")
            .WithSummary("New recovery codes (needs my password; the old ones stop working)")
            .Produces<IReadOnlyCollection<string>>()
            .ProducesValidationProblem();

        return api;
    }

    /// <summary>The body of "change an address": the Id comes from the URL.</summary>
    /// <param name="Label">e.g. "Home", "Work".</param>
    /// <param name="FullAddress">The address as typed.</param>
    /// <param name="Country">Its country.</param>
    /// <param name="RecipientName">Who receives the food; empty = me.</param>
    /// <param name="PhoneNumber">Who the seller calls; empty = my phone.</param>
    public sealed record AddressRequest(string Label, string FullAddress, Country Country,
                                        string? RecipientName = null, string? PhoneNumber = null);

    /// <summary>Body of "accept the Terms" / "seen the Privacy Policy": the version on screen.</summary>
    public sealed record VersionRequest(int Version);
}
