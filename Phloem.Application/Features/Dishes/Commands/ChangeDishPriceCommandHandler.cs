using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

public sealed class ChangeDishPriceCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDishRepository dishes,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeDishPriceCommand, Result>
{
    public async Task<Result> Handle(ChangeDishPriceCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var dish = await dishes.GetByIdAsync(command.DishId, ct);
        if (dish is null || dish.VendorId != vendor.Id) return DishErrors.NotFound;

        var changed = dish.ChangePrice(command.Price);   // inside the range, or PriceOutOfRange
        if (changed.IsFailure) return changed;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}