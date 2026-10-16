using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

public sealed class RemoveDishPhotoCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDishRepository dishes,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveDishPhotoCommand, Result>
{
    public async Task<Result> Handle(RemoveDishPhotoCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var dish = await dishes.GetByIdAsync(command.DishId, ct);
        if (dish is null || dish.VendorId != vendor.Id) return DishErrors.NotFound;

        var removed = dish.RemovePhoto(command.PhotoId);
        if (removed.IsFailure) return removed;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}