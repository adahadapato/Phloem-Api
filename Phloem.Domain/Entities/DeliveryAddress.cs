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
        Set(label, fullAddress, country);
    }

    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string FullAddress { get; private set; } = string.Empty;
    public Country Country { get; private set; }
    public bool IsDefault { get; private set; }

    
    /// <summary>Internal: change addresses through UserProfile.UpdateAddress, so its rules always apply.</summary>
    internal void Update(string label, string fullAddress, Country country) => Set(label, fullAddress, country);

    internal void MarkDefault() => IsDefault = true;
    internal void ClearDefault() => IsDefault = false;

    private void Set(string label, string fullAddress, Country country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullAddress);

        Label = label.Trim();                 // "Home", "Work", "Mum's house"
        FullAddress = fullAddress.Trim();
        Country = country;
    }
}