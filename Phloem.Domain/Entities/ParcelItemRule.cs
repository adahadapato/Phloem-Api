using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One line of the "what can be sent" list shown when booking a parcel, e.g. "Fresh meat or fish" (not allowed) or
/// "Dried fish" (restricted: "sealed, at most 2 kg"). Kept as data so an admin can change the list without a code
/// change or an app update.
/// </summary>
/// <remarks>
/// The list applies to every route. It is shown in <see cref="SortOrder"/> order (lowest first). A line that is no
/// longer needed is really deleted: nothing points at it (a shipment only records that the sender ticked "none of
/// these items"). Bad values are checked by the validators first, so they THROW here.
/// </remarks>
public class ParcelItemRule : Entity<Guid>
{
    /// <summary>Longest item name.</summary>
    public const int NameMaxLength = 100;

    /// <summary>Longest detail (the condition, or why).</summary>
    public const int DetailMaxLength = 300;

    /// <summary>Required by EF Core.</summary>
    protected ParcelItemRule() { }

    private ParcelItemRule(Guid id, ParcelItemRuleKind kind, string name, string? detail, int sortOrder) : base(id)
    {
        Set(kind, name, detail);
        SortOrder = sortOrder;
    }

    /// <summary>Not allowed, or allowed with a condition.</summary>
    public ParcelItemRuleKind Kind { get; private set; }

    /// <summary>e.g. "Fresh meat or fish".</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>The condition or a short reason, e.g. "Sealed, at most 2 kg"; null when there's nothing to add.</summary>
    public string? Detail { get; private set; }

    /// <summary>Position in the list (lowest first).</summary>
    public int SortOrder { get; private set; }

    /// <summary>Adds a line (admin, or the starter data).</summary>
    /// <param name="kind">Not allowed or restricted.</param>
    /// <param name="name">Not empty, at most <see cref="NameMaxLength"/> characters.</param>
    /// <param name="detail">Optional, at most <see cref="DetailMaxLength"/> characters.</param>
    /// <param name="sortOrder">Position in the list (0 or more).</param>
    /// <returns>The new line.</returns>
    /// <exception cref="ArgumentException">Empty name.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown kind, text too long, or a negative position.</exception>
    public static ParcelItemRule Create(ParcelItemRuleKind kind, string name, string? detail, int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        return new ParcelItemRule(Guid.CreateVersion7(), kind, name, detail, sortOrder);
    }

    /// <summary>Changes the line's wording or kind (its position stays).</summary>
    /// <param name="kind">Not allowed or restricted.</param>
    /// <param name="name">Not empty, at most <see cref="NameMaxLength"/> characters.</param>
    /// <param name="detail">Optional, at most <see cref="DetailMaxLength"/> characters.</param>
    /// <exception cref="ArgumentException">Empty name.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown kind or text too long.</exception>
    public void Update(ParcelItemRuleKind kind, string name, string? detail) => Set(kind, name, detail);

    /// <summary>Moves the line to a new position.</summary>
    /// <param name="sortOrder">0 or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">A negative position.</exception>
    public void MoveTo(int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        SortOrder = sortOrder;
    }

    private void Set(ParcelItemRuleKind kind, string name, string? detail)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        var cleanDetail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
        if (cleanDetail is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cleanDetail.Length, DetailMaxLength, nameof(detail));

        Kind = kind;
        Name = name.Trim();
        Detail = cleanDetail;
    }
}
