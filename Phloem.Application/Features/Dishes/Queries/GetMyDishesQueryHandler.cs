using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

public sealed class GetMyDishesQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDishRepository dishes,
    IFileStorage storage)
    : IRequestHandler<GetMyDishesQuery, Result<MyDishesDto>>
{
    public async Task<Result<MyDishesDto>> Handle(GetMyDishesQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var mine = await dishes.GetByVendorAsync(vendor.Id, ct);

        // Dishes needing action first, so the seller sees them straight away
        var items = mine
            .OrderByDescending(d => d.NeedsPriceUpdate)
            .ThenBy(d => d.Name)
            .Select(d => MyDishDto.From(d, storage))
            .ToList();

        return new MyDishesDto(items, items.Count(d => d.NeedsPriceUpdate));
    }
}