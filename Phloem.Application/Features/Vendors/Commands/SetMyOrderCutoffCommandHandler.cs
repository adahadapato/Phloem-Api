using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Handles <see cref="SetMyOrderCutoffCommand"/> for the signed-in seller's shop.</summary>
public sealed class SetMyOrderCutoffCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetMyOrderCutoffCommand, Result>
{
    /// <returns>Success; or <see cref="VendorErrors.NotFound"/>, <see cref="VendorErrors.Blocked"/>.</returns>
    public async Task<Result> Handle(SetMyOrderCutoffCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var set = vendor.SetOrderCutoff(command.Cutoff);
        if (set.IsFailure) return set;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
