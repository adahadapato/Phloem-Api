using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Rejects, saves, emails the shop the reason. Fixing the details sends it back for review.</summary>
public sealed class RejectShopCommandHandler(
    IVendorRepository vendors,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    ILogger<RejectShopCommandHandler> logger)
    : IRequestHandler<RejectShopCommand, Result>
{
    public async Task<Result> Handle(RejectShopCommand command, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(command.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var rejected = vendor.Reject(command.Reason);
        if (rejected.IsFailure) return rejected;

        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await email.SendAsync(vendor.BusinessEmail.Value, "Your Phloem shop needs a change",
                $"<p>We couldn't approve <b>{WebUtility.HtmlEncode(vendor.Name)}</b> yet:</p>" +
                $"<blockquote>{WebUtility.HtmlEncode(command.Reason.Trim())}</blockquote>" +
                "<p>Update your shop in the app and it will come back to us for review.</p>",
                EmailReplyTo.Sellers, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email {Shop} about its rejection.", vendor.Name);
        }

        return Result.Success();
    }
}
