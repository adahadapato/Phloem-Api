using Phloem.Domain.Constants;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A saved delivery address. Belongs to a UserProfile and is only changed through it
/// (so the "exactly one default" rule can't be broken from outside).
/// </summary>
public class DeliveryAddress : Entity<Guid>
{
    protected DeliveryAddress() { }

    internal DeliveryAddress(Guid userId, string label, string fullAddress, Country country)
        : base(Guid.CreateVersion7())
    {
        UserId = userId;
        Label = Rules.Name(label, "Address label", 40);           // "Home", "Work", "Mum's house"
        FullAddress = Rules.Name(fullAddress, "Address", 300);
        Country = country;
    }

    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string FullAddress { get; private set; } = string.Empty;
    public Country Country { get; private set; }
    public bool IsDefault { get; private set; }

    public void Update(string label, string fullAddress, Country country)
    {
        Label = Rules.Name(label, "Address label", 40);
        FullAddress = Rules.Name(fullAddress, "Address", 300);
        Country = country;
    }

    internal void MarkDefault() => IsDefault = true;
    internal void ClearDefault() => IsDefault = false;
}