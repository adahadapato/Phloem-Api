using MediatR;
using Phloem.Application.Features.Commissions.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Commissions.Queries;

/// <summary>Handles <see cref="GetAdminCommissionsQuery"/>.</summary>
public sealed class GetAdminCommissionsQueryHandler(ICommissionRepository commissions)
    : IRequestHandler<GetAdminCommissionsQuery, Result<IReadOnlyList<CommissionDto>>>
{
    /// <summary>Lists every <see cref="Country"/> so the admin sees countries without a rate too.</summary>
    public async Task<Result<IReadOnlyList<CommissionDto>>> Handle(GetAdminCommissionsQuery query, CancellationToken ct)
    {
        var set = (await commissions.GetAllAsync(ct)).ToDictionary(c => c.Country);

        IReadOnlyList<CommissionDto> items = Enum.GetValues<Country>()
            .Select(country => set.TryGetValue(country, out var c)
                ? new CommissionDto(country, c.RatePercent, IsSet: true)
                : new CommissionDto(country, 0m, IsSet: false))
            .ToList();

        return Result<IReadOnlyList<CommissionDto>>.Success(items);
    }
}