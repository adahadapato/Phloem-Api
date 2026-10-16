using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Commands;

public sealed class CreateDishCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDishCatalogRepository catalog,
    IDishRepository dishes,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateDishCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDishCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var name = await catalog.GetByIdAsync(command.DishCatalogId, ct);
        if (name is null) return DishCatalogErrors.NotFound;

        // Domain: approved name, shop not blocked, price inside the admin's range
        var created = Dish.Create(vendor, name, command.Detail, command.Price, command.Description);
        if (created.IsFailure) return created.Error;

        var dish = created.Value;
        dish.SetAvailability(command.IsAvailable);

        dishes.Add(dish);
        await unitOfWork.SaveChangesAsync(ct);

        return dish.Id;
    }
}