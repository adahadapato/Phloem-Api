using MediatR;
using Microsoft.AspNetCore.Mvc;
using Phloem.Application.Features.Chat.Commands;
using Phloem.Application.Features.Chat.Dtos;
using Phloem.Application.Features.Chat.Queries;
using Phloem.Domain.Constants;
using Phloem.Domain.Shared.Results;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authorization;

namespace Phloem.Presentation.Endpoints.Chat;

/// <summary>
/// The order chat: one conversation per order between the customer and the shop, open once the order is paid and
/// until 7 days after it's delivered or cancelled (then read-only).
/// <list type="number">
///   <item><description><b>In an order</b> (<c>/api/orders/{orderNumber}/chat</c>): read, send (text and/or a photo),
///   mark as seen, report a problem. Only the customer and the shop's owner; anyone else gets 404.</description></item>
///   <item><description><b>Mine</b> (<c>/api/me/chats</c>): my conversations and the unread count.</description></item>
///   <item><description><b>Admin</b> (<c>/api/admin/chats</c>, <c>chats.read</c>, the person's countries): read chats
///   (each view is in the audit log) and close reports.</description></item>
/// </list>
/// New messages, "seen" and "typing" arrive live through <c>/hubs/chat</c>.
/// </summary>
public static class ChatEndpoints
{
    /// <summary>Maps the chat endpoints.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder api)
    {
        MapOrderChat(api);
        MapMine(api);
        MapAdmin(api);
        return api;
    }

    // ───────────────────────────── 1. In an order ─────────────────────────────

    private static void MapOrderChat(IEndpointRouteBuilder api)
    {
        var chat = api.MapGroup("/orders/{orderNumber}/chat")
                      .WithTags("Order chat")
                      .RequireAuthorization();

        chat.MapGet("/", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetOrderChatQuery(orderNumber), ct)).ToHttp())
            .WithName("GetOrderChat")
            .WithSummary("The chat with its 50 newest messages (empty until someone writes)")
            .Produces<OrderChatDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        chat.MapGet("/messages", async (string orderNumber, DateTime before, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetOlderChatMessagesQuery(orderNumber, before), ct)).ToHttp())
            .WithName("GetOlderChatMessages")
            .WithSummary("Older messages: the 50 before a time (\"Show earlier messages\")")
            .Produces<IReadOnlyList<ChatMessageDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        chat.MapPost("/", async (string orderNumber, [FromForm] string? body, IFormFile? photo, ISender sender,
                                 CancellationToken ct) =>
        {
            await using var content = photo?.OpenReadStream() ?? Stream.Null;
            return (await sender.Send(new SendChatMessageCommand(orderNumber, body, content, photo?.Length ?? 0), ct))
                .ToHttp();
        })
            .WithName("SendChatMessage")
            .WithSummary("Send a message: text (\"body\"), a photo (\"photo\": JPEG, PNG, WebP or HEIC, ≤ 10 MB), or both")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ChatMessageDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .DisableAntiforgery();   // JWT API: no cookies, so no cross-site form risk

        chat.MapPost("/read", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new MarkChatReadCommand(orderNumber), ct)).ToHttp())
            .WithName("MarkChatRead")
            .WithSummary("I've seen the chat (the other side sees \"Seen\")")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        chat.MapPost("/report", async (string orderNumber, ReportRequest request, ISender sender, CancellationToken ct) =>
                (await sender.Send(new ReportChatCommand(orderNumber, request.Reason), ct)).ToHttp())
            .WithName("ReportChat")
            .WithSummary("Report a problem: Phloem's team reads the chat")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        api.MapDelete("/chat/messages/{messageId:guid}", async (Guid messageId, ISender sender, CancellationToken ct) =>
                (await sender.Send(new HideChatMessageCommand(messageId), ct)).ToHttp())
            .WithTags("Order chat")
            .RequireAuthorization()
            .WithName("HideChatMessage")
            .WithSummary("Delete one of my messages for me (the other side still sees it)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    // ───────────────────────────── 2. Mine ─────────────────────────────

    private static void MapMine(IEndpointRouteBuilder api)
    {
        var mine = api.MapGroup("/me/chats")
                      .WithTags("Order chat")
                      .RequireAuthorization();

        mine.MapGet("/", async ([AsParameters] GetMyChatsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetMyChats")
            .WithSummary("My conversations (as a customer and as a shop), latest first")
            .Produces<PagedResult<MyChatDto>>()
            .ProducesValidationProblem();

        mine.MapGet("/unread", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetMyUnreadChatCountQuery(), ct)).ToHttp())
            .WithName("GetMyUnreadChatCount")
            .WithSummary("How many conversations have messages I haven't seen (the number in the header)")
            .Produces<int>();
    }

    // ───────────────────────────── 3. Admin ─────────────────────────────

    private static void MapAdmin(IEndpointRouteBuilder api)
    {
        var admin = api.MapGroup("/admin/chats")
                       .WithTags("Admin: Order chats")
                       .RequirePermission(Permissions.Messages.ReadChats, CountryRule.RecordCountry);

        admin.MapGet("/", async ([AsParameters] GetAdminChatsQuery query, ISender sender, CancellationToken ct) =>
                (await sender.Send(query, ct)).ToHttp())
            .WithName("GetAdminChats")
            .WithSummary("Chats in your countries, latest first (reportedOnly = waiting reports)")
            .Produces<PagedResult<AdminChatSummaryDto>>()
            .ProducesValidationProblem();

        admin.MapGet("/{orderNumber}", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetAdminChatQuery(orderNumber), ct)).ToHttp())
            .WithName("GetAdminChat")
            .WithSummary("The whole chat with full names (the view is recorded in the audit log)")
            .Produces<AdminChatDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPost("/{orderNumber}/close-report", async (string orderNumber, ISender sender, CancellationToken ct) =>
                (await sender.Send(new CloseChatReportCommand(orderNumber), ct)).ToHttp())
            .WithName("CloseChatReport")
            .WithSummary("The report has been dealt with")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>Body of "report a problem".</summary>
    /// <param name="Reason">What's wrong.</param>
    public sealed record ReportRequest(string Reason);
}
