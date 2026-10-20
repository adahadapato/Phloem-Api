using MediatR;
using Phloem.Application.Features.Countries.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Countries.Queries;

/// <summary>
/// Lists every value of <see cref="Country"/> (a new country appears as soon as it's added to the enum and its
/// <see cref="CountryExtensions"/>), with the parcel destinations of its switched-on routes.
/// </summary>
public sealed class GetCountriesQueryHandler(IShipmentRouteRepository routes)
    : IRequestHandler<GetCountriesQuery, Result<IReadOnlyList<CountryDto>>>
{
    public async Task<Result<IReadOnlyList<CountryDto>>> Handle(GetCountriesQuery query, CancellationToken ct)
    {
        var active = await routes.GetActiveAsync(ct);

        IReadOnlyList<CountryDto> countries = Enum.GetValues<Country>()
            .Select(country => CountryDto.From(country,
                active.Where(r => r.FromCountry == country).Select(r => r.ToCountry).Distinct().ToList()))
            .ToList();

        return Result<IReadOnlyList<CountryDto>>.Success(countries);
    }
}
