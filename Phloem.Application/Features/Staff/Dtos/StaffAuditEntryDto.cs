using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Staff.Dtos;

/// <summary>One line of the staff audit log.</summary>
/// <param name="Id">The line.</param>
/// <param name="At">When it happened (UTC).</param>
/// <param name="ActorId">Who did it.</param>
/// <param name="ActorName">Their name ("Unknown user" if the account is gone).</param>
/// <param name="Action">What they did.</param>
/// <param name="Target">What it was done to, e.g. "Parcel PHS-261102-000123".</param>
/// <param name="Country">The record's country, if any.</param>
/// <param name="Detail">A short note.</param>
public sealed record StaffAuditEntryDto(
    Guid Id,
    DateTime At,
    Guid ActorId,
    string ActorName,
    StaffAction Action,
    string Target,
    Country? Country,
    string? Detail)
{
    /// <summary>Builds the line from the stored entry and the actor's name.</summary>
    /// <param name="entry">The stored entry.</param>
    /// <param name="actorName">The actor's name.</param>
    /// <returns>The DTO.</returns>
    public static StaffAuditEntryDto From(StaffAuditEntry entry, string actorName) => new(
        entry.Id, entry.CreatedAt, entry.ActorId, actorName, entry.Action, entry.Target, entry.Country,
        entry.Detail);
}
