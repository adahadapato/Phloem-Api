using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.ParcelItemRules.Dtos;

/// <summary>One line of the "what can be sent" list.</summary>
/// <param name="Id">For the admin's edit / delete / reorder.</param>
/// <param name="Kind">NotAllowed or Restricted (allowed only as <paramref name="Detail"/> says).</param>
/// <param name="Name">e.g. "Fresh meat or fish".</param>
/// <param name="Detail">The condition or a short reason; null when there's nothing to add.</param>
/// <param name="SortOrder">Position (the list already comes in this order).</param>
public sealed record ParcelItemRuleDto(Guid Id, ParcelItemRuleKind Kind, string Name, string? Detail, int SortOrder)
{
    /// <summary>Maps a line.</summary>
    /// <param name="r">The line.</param>
    /// <returns>The DTO.</returns>
    public static ParcelItemRuleDto From(ParcelItemRule r) => new(r.Id, r.Kind, r.Name, r.Detail, r.SortOrder);
}
