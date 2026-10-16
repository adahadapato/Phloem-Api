using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Commands;

public sealed class SuggestDishNameCommandHandler(
    ICurrentUser currentUser,
    IDishCatalogRepository catalog,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SuggestDishNameCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SuggestDishNameCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        if (await catalog.NameExistsAsync(command.Name.Trim(), ct))
            return DishCatalogErrors.NameTaken;

        var suggestion = DishCatalog.Suggest(command.Name, command.Category, command.Group, userId);

        catalog.Add(suggestion);
        await unitOfWork.SaveChangesAsync(ct);

        return suggestion.Id;
    }
}