using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Accounts.Dtos;

public sealed record AddressDto(
    Guid Id,
    string Label,
    string FullAddress,
    Country Country,
    bool IsDefault)
{
    public static AddressDto From(DeliveryAddress address) => new(
        address.Id,
        address.Label,
        address.FullAddress,
        address.Country,
        address.IsDefault);
}