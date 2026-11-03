namespace Phloem.Domain.Enums;

/// <summary>Which kind of website page an SEO entry is for.</summary>
public enum SeoPageKind
{
    /// <summary>A fixed page: Home, Explore, Shops, Send a parcel… (listed in <see cref="Constants.SeoPages"/>).</summary>
    Main = 1,

    /// <summary>A shop's page (<c>/shops/{id}</c>).</summary>
    Shop = 2
}
