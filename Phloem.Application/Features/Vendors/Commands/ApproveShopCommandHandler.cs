using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Approve + give the owner the <see cref="Roles.Seller"/> role in ONE transaction (both or neither), then email the
/// shop. The owner sees the role at their next token refresh.
/// </summary>
public sealed class ApproveShopCommandHandler(
    IVendorRepository vendors,
    IUserRoleService roles,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    TimeProvider time,
    ILogger<ApproveShopCommandHandler> logger)
    : IRequestHandler<ApproveShopCommand, Result>
{
    public async Task<Result> Handle(ApproveShopCommand command, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(command.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        await using (var transaction = await unitOfWork.BeginTransactionAsync(ct))
        {
            var approved = vendor.Approve(time.GetUtcNow().UtcDateTime);
            if (approved.IsFailure) return approved;

            await unitOfWork.SaveChangesAsync(ct);

            var role = await roles.AddToRoleAsync(vendor.OwnerId, Roles.Seller, ct);
            if (role.IsFailure) return role;   // no commit = rollback

            await transaction.CommitAsync(ct);
        }

        // After the commit: a failed email never undoes the approval
        try
        {
            await email.SendAsync(vendor.BusinessEmail.Value, "Your Phloem shop is live!",
                $"<p>Good news: <b>{WebUtility.HtmlEncode(vendor.Name)}</b> has been approved and buyers can now see it.</p>" +
                "<p>Sign in again in the app to open your Seller dashboard.</p>",
                EmailReplyTo.Sellers, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email {Shop} about its approval.", vendor.Name);
        }

        return Result.Success();
    }
}
