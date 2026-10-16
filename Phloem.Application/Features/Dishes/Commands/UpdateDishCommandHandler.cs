using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

public sealed class UpdateDishCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDishCatalogRepository catalog,
    IDishRepository dishes,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateDishCommand, Result>
{
    public async Task<Result> Handle(UpdateDishCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var dish = await dishes.GetByIdAsync(command.DishId, ct);
        if (dish is null || dish.VendorId != vendor.Id) return DishErrors.NotFound;   // only YOUR dishes

        var name = await catalog.GetByIdAsync(command.DishCatalogId, ct);
        if (name is null) return DishCatalogErrors.NotFound;

        var updated = dish.Update(name, command.Detail, command.Price, command.Description);
        if (updated.IsFailure) return updated;

        dish.SetAvailability(command.IsAvailable);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}