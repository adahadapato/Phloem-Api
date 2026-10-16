using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.DeliveryPricings.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Queries;

/// <summary>
/// The app's "Delivery time" picker: which windows are offered in a country and what each adds.
/// Public and cached; cleared whenever an admin changes delivery pricing.
/// </summary>
/// <param name="Country">The country the customer is shopping in.</param>
[Cacheable(CacheTags.DeliveryPricing, Minutes = 60)]
public sealed record GetDeliveryWindowsQuery(Country Country)
    : IRequest<Result<IReadOnlyList<DeliveryWindowOptionDto>>>;