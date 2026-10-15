using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A name in the fixed dish list sellers pick from ("Jollof Rice", "Waakye"…), so there are no typos.
/// Sellers can suggest a missing name; an admin approves or rejects it.
/// </summary>
public class DishCatalog : Entity<Guid>
{
    protected DishCatalog() { }

    private DishCatalog(Guid id, string name, DishCategory category, CatalogGroup group,
                        DishCatalogStatus status, Guid? suggestedByUserId)
        : base(id)
    {
        Name = name;
        Category = category;
        Group = group;
        Status = status;
        SuggestedByUserId = suggestedByUserId;
    }

    public string Name { get; private set; } = string.Empty;      // unique (case-insensitive)
    public DishCategory Category { get; private set; }
    public CatalogGroup Group { get; private set; }
    public DishCatalogStatus Status { get; private set; }

    public Guid? SuggestedByUserId { get; private set; }            // null for the starter list
    public DateTime? ReviewedAtUtc { get; private set; }

    public bool CanBeUsed => Status == DishCatalogStatus.Approved;

    /// <summary>Starter list / admin-added names: approved straight away.</summary>
    public static DishCatalog Create(string name, DishCategory category, CatalogGroup group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new(Guid.CreateVersion7(), name.Trim(), category, group, DishCatalogStatus.Approved, null);
    }

    /// <summary>"Can't find your dish?" — waits for an admin.</summary>
    public static DishCatalog Suggest(string name, DishCategory category, CatalogGroup group, Guid suggestedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new(Guid.CreateVersion7(), name.Trim(), category, group, DishCatalogStatus.Suggested, suggestedByUserId);
    }

    public void Approve(DateTime utcNow)
    {
        if (Status != DishCatalogStatus.Suggested) throw new InvalidOperationException("Only suggested names can be approved.");
        Status = DishCatalogStatus.Approved;
        ReviewedAtUtc = utcNow;
    }

    public void Reject(DateTime utcNow)
    {
        if (Status != DishCatalogStatus.Suggested) throw new InvalidOperationException("Only suggested names can be rejected.");
        Status = DishCatalogStatus.Rejected;
        ReviewedAtUtc = utcNow;
    }
}