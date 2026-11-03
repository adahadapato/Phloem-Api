using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces;

namespace Phloem.Infrastructure.Settings.Application;

/// <summary>Implements <see cref="ISiteAddresses"/> from the <c>AppSettings</c> section.</summary>
internal sealed class SiteAddresses(IOptions<ApplicationSettings> options) : ISiteAddresses
{
    /// <inheritdoc />
    public string Website => options.Value.WebsiteBaseUrl.TrimEnd('/');

    /// <inheritdoc />
    public string Api => options.Value.ApiBaseUrl.TrimEnd('/');
}
