using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Application.Features.Chat.Common;
using Phloem.Application.Features.Chat.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Chat.Commands;

/// <summary>Handles <see cref="SendChatMessageCommand"/>.</summary>
public sealed class SendChatMessageCommandHandler(
    ICurrentUser currentUser,
    ChatAccess access,
    IOrderChatRepository chats,
    IImageProcessor images,
    IFileStorage storage,
    ChatFormatting format,
    IChatRealtime realtime,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<SendChatMessageCommandHandler> logger)
    : IRequestHandler<SendChatMessageCommand, Result<ChatMessageDto>>
{
    /// <summary>Saves the photo (private, GPS removed), the message, and tells both sides' open pages.</summary>
    /// <returns>The message; or NotFound / Closed, or an image error the sender can fix.</returns>
    public async Task<Result<ChatMessageDto>> Handle(SendChatMessageCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var found = await access.ForParticipantAsync(command.OrderNumber, userId, ct);
        if (found.IsFailure) return found.Error;
        var (order, chat, role, ownerId) = found.Value;

        var now = time.GetUtcNow().UtcDateTime;
        if (!OrderChat.IsOpen(order, now)) return ChatErrors.Closed;

        // 1. The photo (processed like delivery photos: upright, ≤ 1600 px, GPS removed, WebP, private)
        string? photoPath = null;
        if (command.PhotoSizeBytes > 0)
        {
            var processed = await images.ProcessDeliveryPhotoAsync(command.Photo, ct);
            if (processed.IsFailure) return processed.Error;
            await using var image = processed.Value;
            photoPath = $"chats/{now:yyyy}/{now:MM}/{order.OrderNumber}-{Guid.CreateVersion7():N}.webp";
            await storage.SaveAsync(image.FullSize, photoPath, image.ContentType, StorageContainer.Private, ct);
        }

        // 2. The message (the chat starts with the first one)
        if (chat is null)
        {
            chat = OrderChat.Start(order, ownerId);
            chats.Add(chat);
        }
        var message = ChatMessage.Write(chat, userId, role, command.Body, photoPath);
        chats.AddMessage(message);
        chat.MessageWritten(role, now);
        await unitOfWork.SaveChangesAsync(ct);

        // 3. Live to both sides (the sender's other tabs and phone too)
        var (customerName, ownerName) = await format.FullNamesAsync(order.CustomerId, ownerId, ct);
        var dto = await format.ForParticipantAsync(message, customerName, ownerName, order.VendorName, ct);
        try
        {
            await realtime.MessageAsync(new ChatMessageEvent(order.OrderNumber, dto), [order.CustomerId, ownerId], ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Couldn't push chat message {MessageId} live; pages will load it when opened.", message.Id);
        }
        return dto;
    }
}

/// <summary>Handles <see cref="MarkChatReadCommand"/>.</summary>
public sealed class MarkChatReadCommandHandler(
    ICurrentUser currentUser,
    ChatAccess access,
    IChatRealtime realtime,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<MarkChatReadCommand, Result>
{
    /// <summary>Moves my "seen" marker and tells the other side (their "Seen" ticks).</summary>
    public async Task<Result> Handle(MarkChatReadCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var found = await access.ForParticipantAsync(command.OrderNumber, userId, ct);
        if (found.IsFailure) return found.Error;
        var (order, chat, role, _) = found.Value;
        if (chat is null) return Result.Success();

        var now = time.GetUtcNow().UtcDateTime;
        if (!chat.MarkRead(role, now)) return Result.Success();
        await unitOfWork.SaveChangesAsync(ct);

        try { await realtime.ReadAsync(new ChatReadEvent(order.OrderNumber, role, now), chat.OtherParty(role), ct); }
        catch (Exception ex) when (ex is not OperationCanceledException) { /* best effort */ _ = ex; }
        return Result.Success();
    }
}

/// <summary>Handles <see cref="HideChatMessageCommand"/>.</summary>
public sealed class HideChatMessageCommandHandler(ICurrentUser currentUser, IOrderChatRepository chats, IUnitOfWork unitOfWork)
    : IRequestHandler<HideChatMessageCommand, Result>
{
    /// <returns>Success; or <see cref="ChatErrors.MessageNotFound"/> (not mine).</returns>
    public async Task<Result> Handle(HideChatMessageCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var message = await chats.GetMessageAsync(command.MessageId, ct);
        if (message is null || message.SenderId != userId) return ChatErrors.MessageNotFound;

        message.HideForSender();
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Handles <see cref="ReportChatCommand"/>.</summary>
public sealed class ReportChatCommandHandler(
    ICurrentUser currentUser,
    ChatAccess access,
    IOrderChatRepository chats,
    IEmailSender email,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<ReportChatCommandHandler> logger)
    : IRequestHandler<ReportChatCommand, Result>
{
    /// <summary>Marks the chat reported (starting it if nobody has written) and tells the team inbox.</summary>
    /// <returns>Success; or NotFound / AlreadyReported.</returns>
    public async Task<Result> Handle(ReportChatCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var found = await access.ForParticipantAsync(command.OrderNumber, userId, ct);
        if (found.IsFailure) return found.Error;
        var (order, chat, role, ownerId) = found.Value;

        if (chat is null)
        {
            chat = OrderChat.Start(order, ownerId);
            chats.Add(chat);
        }
        if (chat.ReportWaiting) return ChatErrors.AlreadyReported;

        chat.Report(userId, command.Reason, time.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await email.SendToTeamAsync($"Chat reported: order {order.OrderNumber}",
                $"<p>The {(role == ChatRole.Customer ? "customer" : "shop")} reported the chat about order " +
                $"<b>{WebUtility.HtmlEncode(order.OrderNumber)}</b> ({WebUtility.HtmlEncode(order.VendorName)}):</p>" +
                $"<blockquote>{WebUtility.HtmlEncode(command.Reason.Trim())}</blockquote>" +
                "<p>Open <b>Admin → Order chats</b> to read it.</p>", ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email the team about the reported chat {OrderNumber}.", order.OrderNumber);
        }
        return Result.Success();
    }
}

/// <summary>Handles <see cref="CloseChatReportCommand"/>.</summary>
public sealed class CloseChatReportCommandHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IOrderChatRepository chats,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<CloseChatReportCommand, Result>
{
    /// <returns>Success; or <see cref="ChatErrors.NotFound"/>.</returns>
    public async Task<Result> Handle(CloseChatReportCommand command, CancellationToken ct)
    {
        var staffId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var order = await orders.GetByOrderNumberAsync(command.OrderNumber.Trim(), ct);
        var chat = order is null ? null : await chats.GetByOrderIdAsync(order.Id, ct);
        if (chat is null || !currentUser.CoversCountry(chat.Country)) return ChatErrors.NotFound;
        if (!chat.ReportWaiting) return Result.Success();

        chat.CloseReport(staffId, time.GetUtcNow().UtcDateTime);
        audit.Add(StaffAuditEntry.Create(staffId, StaffAction.ChatReportClosed, $"Chat {chat.OrderNumber}", chat.Country));
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>Handles <see cref="SendChatEmailsCommand"/>.</summary>
/// <remarks>Respects people's email settings: customers' "Order updates", shops' "Shop orders". One email per unread
/// batch, at most every few minutes per chat.</remarks>
public sealed class SendChatEmailsCommandHandler(
    IOrderChatRepository chats,
    IUserProfileRepository profiles,
    IAccountService accounts,
    IEmailSender email,
    ISiteAddresses site,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<SendChatEmailsCommandHandler> logger)
    : IRequestHandler<SendChatEmailsCommand, Result<int>>
{
    /// <summary>How long a message waits unread before an email goes out.</summary>
    public static readonly TimeSpan Delay = TimeSpan.FromMinutes(5);

    /// <returns>How many emails were sent.</returns>
    public async Task<Result<int>> Handle(SendChatEmailsCommand command, CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var cutoff = now - Delay;
        var sent = 0;

        foreach (var chat in await chats.GetMaybeNeedingEmailAsync(cutoff, 50, ct))
        {
            foreach (var role in new[] { ChatRole.Customer, ChatRole.Seller })
            {
                if (!chat.NeedsEmail(role, cutoff)) continue;
                chat.Emailed(role, now);   // marked even if they don't want emails, so it isn't checked again

                var recipient = role == ChatRole.Customer ? chat.CustomerId : chat.ShopOwnerId;
                var profile = await profiles.GetByIdAsync(recipient, ct);
                if (profile is null || !(role == ChatRole.Customer ? profile.NotifyOrderUpdates : profile.NotifyShopOrders))
                    continue;
                var contact = await accounts.GetContactAsync(recipient, ct);
                if (contact.IsFailure) continue;

                var from = role == ChatRole.Customer ? WebUtility.HtmlEncode(chat.ShopName) : "Your customer";
                var link = $"{site.Website}/messages?chat={Uri.EscapeDataString(chat.OrderNumber)}";
                try
                {
                    await email.SendAsync(contact.Value.Email, $"New message about order {chat.OrderNumber}",
                        $"<p>{from} sent you a message about order <b>{WebUtility.HtmlEncode(chat.OrderNumber)}</b>.</p>" +
                        $"<p><a href=\"{WebUtility.HtmlEncode(link)}\">Open the chat</a> to read it and reply.</p>" +
                        "<p style=\"color:#666\">You can switch these emails off in your account's notification settings.</p>",
                        role == ChatRole.Customer ? EmailReplyTo.Support : EmailReplyTo.Sellers, ct);
                    sent++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Couldn't email about chat {OrderNumber}.", chat.OrderNumber);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return sent;
    }
}
