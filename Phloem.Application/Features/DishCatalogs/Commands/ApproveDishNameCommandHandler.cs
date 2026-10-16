using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

public sealed class ApproveDishNameCommandHandler(
    IDishCatalogRepository catalog,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<ApproveDishNameCommand, Result>
{
    public async Task<Result> Handle(ApproveDishNameCommand command, CancellationToken ct)
    {
        var item = await catalog.GetByIdAsync(command.DishCatalogId, ct);
        if (item is null) return DishCatalogErrors.NotFound;

        var approved = item.Approve(time.GetUtcNow().UtcDateTime);
        if (approved.IsFailure) return approved;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}