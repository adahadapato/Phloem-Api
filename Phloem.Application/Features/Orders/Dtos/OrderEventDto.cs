using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>One line of an order's timeline, as the viewer may see it.</summary>
/// <param name="Kind">What happened.</param>
/// <param name="StatusAfter">The order's status straight after.</param>
/// <param name="At">When (UTC).</param>
/// <param name="ActorKind">What kind of person did it.</param>
/// <param name="ActorName">Who: full name for staff viewers; first name and role for customers and sellers, e.g.
/// "Kofi (courier)", "Ama (Phloem)"; "Phloem" when Phloem did it.</param>
/// <param name="ActorRole">"Customer", "Shop", "Courier", "Phloem staff" or "Phloem".</param>
/// <param name="ActorId">Their account, for staff viewers only (null for everyone else).</param>
/// <param name="Note">A short note, e.g. "Courier: Kofi Asante" or the decline reason.</param>
/// <param name="IsRebuilt">True for lines rebuilt from an older order's saved dates.</param>
public sealed record OrderEventDto(
    OrderEventKind Kind,
    OrderStatus StatusAfter,
    DateTime At,
    OrderActorKind ActorKind,
    string ActorName,
    string ActorRole,
    Guid? ActorId,
    string? Note,
    bool IsRebuilt);
