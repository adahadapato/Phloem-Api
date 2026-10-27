using MediatR;
using Phloem.Application.Features.Contact.Commands;
using Phloem.Application.Features.Contact.Dtos;
using Phloem.Application.Features.Contact.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.RateLimiting;

namespace Phloem.Presentation.Endpoints.ContactUs;

/// <summary>
/// Contact us:
/// <list type="bullet">
///   <item><description><b>Anyone</b> (<c>/api/contact</c>): the contact details, and sending a message (rate limited).</description></item>
///   <item><description><b>Team</b> (<c>/api/admin/contact-messages</c>): the inbox and marking messages. Admins and
///   support agents.</description></item>
/// </list>
/// </summary>
public static class ContactEndpoints
{
    /// <summary>Maps the Contact us endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder api)
    {
        MapPublic(api);
        MapTeam(api);
        return api;
    }

    // ═════════════════════════════ A. Anyone ═════════════════════════════

    private static void MapPublic(IEndpointRouteBuilder api)
    {
        var contact = api.MapGroup("/contact")
                         .WithTags("Contact us");

        contact.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetContactDetailsQuery(), ct)).ToHttp())
            .WithName("GetContactDetails")
            .WithSummary("Contact us page: support email, phone, WhatsApp, address, opening hours, reply time")
            .WithDescription("Values that aren't set are null: leave them off the page.")
            .Produces<ContactDetailsDto>();

        // Signed in or not: when signed in, the message is linked to the account
        contact.MapPost("/messages", async (SendContactMessageCommand command, ISender sender, CancellationToken ct) =>
                (await sender.Send(command, ct)).ToHttp())
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingSetup.ContactPolicy)
            .WithName("SendContactMessage")
            .WithSummary("Send a Contact us message: saved for the team, emailed to them, confirmation to the sender")
            .WithDescription($"Topic = Order | Parcel | Selling | Account | Other. At most {RateLimitingSetup.ContactPermitLimit} " +
                             $"messages every {RateLimitingSetup.ContactWindow.TotalMinutes:0} minutes from one connection (429 after that).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);
    }

    // ═════════════════════════════ B. Team ═════════════════════════════

    private static void MapTeam(IEndpointRouteBuilder api)
    {
        var inbox = api.MapGroup("/admin/contact-messages")
                       .WithTags("Admin: Contact messages")
                       .RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.SupportAgent));

        inbox.MapGet("/", async ([AsParameters] GetContactMessagesQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetContactMessages")
            .WithSummary("The inbox, newest first; optionally one status (New, Replied, Closed); with counts per status")
            .Produces<ContactInboxDto>()
            .ProducesValidationProblem();

        inbox.MapPut("/{id:guid}/status", async (Guid id, ContactStatusRequest body, ISender sender, CancellationToken ct) =>
                (await sender.Send(new UpdateContactMessageStatusCommand(id, body.Status, body.Note), ct)).ToHttp())
            .WithName("UpdateContactMessageStatus")
            .WithSummary("Mark a message Replied, Closed or back to New, with an optional note for colleagues")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ═════════════════════════════ Request bodies ═════════════════════════════

    /// <summary>Body of "change a message's status" (the message is in the URL).</summary>
    public sealed record ContactStatusRequest(ContactMessageStatus Status, string? Note);
}
