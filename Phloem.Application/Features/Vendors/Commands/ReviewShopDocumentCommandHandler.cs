using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Records the review with the admin's Id; a rejection is emailed to the shop with the reason.</summary>
public sealed class ReviewShopDocumentCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    TimeProvider time,
    ILogger<ReviewShopDocumentCommandHandler> logger)
    : IRequestHandler<ReviewShopDocumentCommand, Result>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(ReviewShopDocumentCommand command, CancellationToken ct)
    {
        var adminId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByIdAsync(command.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var now = time.GetUtcNow().UtcDateTime;
        var reviewed = command.Accept
            ? vendor.AcceptDocument(command.DocumentId, adminId, now)
            : vendor.RejectDocument(command.DocumentId, adminId, command.Reason!, now);   // validator: reason given
        if (reviewed.IsFailure) return reviewed;

        await unitOfWork.SaveChangesAsync(ct);

        if (!command.Accept)
        {
            var type = vendor.Documents.First(d => d.Id == command.DocumentId).Type;
            try
            {
                await email.SendAsync(vendor.BusinessEmail.Value, "Please upload a new document",
                    $"<p>Your <b>{type}</b> document for <b>{WebUtility.HtmlEncode(vendor.Name)}</b> wasn't accepted:</p>" +
                    $"<blockquote>{WebUtility.HtmlEncode(command.Reason!.Trim())}</blockquote>" +
                    "<p>Please upload a new copy in the app.</p>",
                    EmailReplyTo.Sellers, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Couldn't email {Shop} about a rejected document.", vendor.Name);
            }
        }

        return Result.Success();
    }
}
