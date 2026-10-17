using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Commissions.Commands;

/// <summary>
/// Admin sets a country's commission rate (creates it the first time). Only orders delivered
/// from now on use the new rate.
/// </summary>
/// <param name="Country">The country.</param>
/// <param name="RatePercent">0 to Commission.MaxRatePercent, e.g. 10 = 10% of the dishes.</param>
public sealed record SetCommissionCommand(Country Country, decimal RatePercent) : IRequest<Result>;