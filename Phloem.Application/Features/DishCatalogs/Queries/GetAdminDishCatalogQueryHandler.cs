using MediatR;
using Phloem.Application.Features.DishCatalogs.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DishCatalogs.Queries;

public sealed class GetAdminDishCatalogQueryHandler(IDishCatalogRepository catalog)
    : IRequestHandler<GetAdminDishCatalogQuery, Result<IReadOnlyList<AdminDishCatalogDto>>>
{
    public async Task<Result<IReadOnlyList<AdminDishCatalogDto>>> Handle(GetAdminDishCatalogQuery query, CancellationToken ct)
    {
        var approved = await catalog.GetApprovedAsync(ct);

        IReadOnlyList<AdminDishCatalogDto> items = approved.Select(AdminDishCatalogDto.From).ToList();
        return Result<IReadOnlyList<AdminDishCatalogDto>>.Success(items);
    }
}