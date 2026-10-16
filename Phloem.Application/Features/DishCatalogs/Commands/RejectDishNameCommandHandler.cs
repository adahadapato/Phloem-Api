using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

public sealed class RejectDishNameCommandHandler(
    IDishCatalogRepository catalog,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<RejectDishNameCommand, Result>
{
    public async Task<Result> Handle(RejectDishNameCommand command, CancellationToken ct)
    {
        var item = await catalog.GetByIdAsync(command.DishCatalogId, ct);
        if (item is null) return DishCatalogErrors.NotFound;

        var rejected = item.Reject(time.GetUtcNow().UtcDateTime);
        if (rejected.IsFailure) return rejected;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}