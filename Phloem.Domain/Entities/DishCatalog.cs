using Phloem.Domain.Enums;
using Phloem.Domain.Errors;
using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Entities;

/// <summary>
/// A name in the fixed dish list sellers pick from ("Jollof Rice", "Waakye"…), so there are no typos.
/// Sellers can suggest a missing name; an admin approves or rejects it.
///
/// Errors: business-rule failures are RETURNED as a Result (see DishCatalogErrors);
/// a blank name (the validator checks first) still THROWS.
/// Duplicate names are checked by the Application layer with IDishCatalogRepository.NameExistsAsync.
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

    /// <summary>Admin approves a suggestion: it joins the list every seller picks from.</summary>
    public Result Approve(DateTime utcNow)
    {
        if (Status != DishCatalogStatus.Suggested) return DishCatalogErrors.AlreadyReviewed;

        Status = DishCatalogStatus.Approved;
        ReviewedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>Admin rejects a suggestion. It can be suggested again later.</summary>
    public Result Reject(DateTime utcNow)
    {
        if (Status != DishCatalogStatus.Suggested) return DishCatalogErrors.AlreadyReviewed;

        Status = DishCatalogStatus.Rejected;
        ReviewedAtUtc = utcNow;
        return Result.Success();
    }
}