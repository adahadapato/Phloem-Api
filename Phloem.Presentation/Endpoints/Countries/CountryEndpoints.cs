using MediatR;
using Phloem.Application.Features.Countries.Dtos;
using Phloem.Application.Features.Countries.Queries;
using Phloem.Presentation.Extensions;

namespace Phloem.Presentation.Endpoints.Countries;

/// <summary>
/// <c>/api/countries</c>: the countries Phloem works in, with names, flags, currencies and where parcels can go.
/// The app and the website build their country pickers from this, so neither keeps its own list.
/// </summary>
/// <remarks>Open to anyone: the pickers show before sign-in.</remarks>
public static class CountryEndpoints
{
    /// <summary>Maps the country endpoints under the given group.</summary>
    /// <param name="api">The <c>/api</c> group.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IEndpointRouteBuilder MapCountryEndpoints(this IEndpointRouteBuilder api)
    {
        var countries = api.MapGroup("/countries")
                           .WithTags("Countries");

        countries.MapGet("/", async (ISender sender, CancellationToken ct) =>
                (await sender.Send(new GetCountriesQuery(), ct)).ToHttp())
            .WithName("GetCountries")
            .WithSummary("Countries for the pickers: name, flag, currency, and where parcels can be sent")
            .Produces<IReadOnlyList<CountryDto>>();

        return api;
    }
}
