using MediatR;
using Phloem.Application.Features.ParcelItemRules.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Queries;

/// <summary>Handles <see cref="GetParcelItemRulesQuery"/>.</summary>
public sealed class GetParcelItemRulesQueryHandler(IParcelItemRuleRepository rules)
    : IRequestHandler<GetParcelItemRulesQuery, Result<IReadOnlyList<ParcelItemRuleDto>>>
{
    /// <summary>Every line, lowest position first (an empty list is fine).</summary>
    public async Task<Result<IReadOnlyList<ParcelItemRuleDto>>> Handle(GetParcelItemRulesQuery query,
                                                                         CancellationToken ct)
    {
        var all = await rules.GetAllAsync(ct);
        IReadOnlyList<ParcelItemRuleDto> items = all.Select(ParcelItemRuleDto.From).ToList();
        return Result<IReadOnlyList<ParcelItemRuleDto>>.Success(items);
    }
}
