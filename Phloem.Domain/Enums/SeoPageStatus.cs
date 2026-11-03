namespace Phloem.Domain.Enums;

/// <summary>Where a page's search engine text stands (Admin → SEO).</summary>
public enum SeoPageStatus
{
    /// <summary>Nothing written yet: Google gets Phloem's simple default (e.g. the shop's name).</summary>
    NotWritten = 1,

    /// <summary>Written by the AI.</summary>
    WrittenByAi = 2,

    /// <summary>Written or changed by staff (the AI leaves it alone).</summary>
    EditedByStaff = 3
}
