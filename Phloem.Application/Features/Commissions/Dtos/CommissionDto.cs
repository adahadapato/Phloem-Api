using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Commissions.Dtos;

/// <summary>Admin → Commissions page: one country's rate.</summary>
/// <param name="Country">The country.</param>
/// <param name="RatePercent">The rate (0 = none set or 0%).</param>
/// <param name="IsSet">False if the admin never set a rate for this country (0% applies).</param>
public sealed record CommissionDto(Country Country, decimal RatePercent, bool IsSet);