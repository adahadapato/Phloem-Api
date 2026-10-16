using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>
/// Admin stops offering a delivery window in a country. Unpaid orders that chose it will need
/// another window when they're paid.
/// </summary>
/// <param name="Country">The country.</param>
/// <param name="Window">The window to stop offering.</param>
[InvalidatesCache(CacheTags.DeliveryPricing)]
public sealed record RemoveDeliveryWindowCommand(Country Country, DeliveryWindow Window) : IRequest<Result>;