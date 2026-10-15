using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The business side of a Phloem account: name, country, photo, delivery addresses,
/// notification switches. It has the SAME Id as the login account (ApplicationUser in
/// Persistence), which owns email, phone, password and roles.
/// </summary>
public class UserProfile : Entity<Guid>
{
    private readonly List<DeliveryAddress> _addresses = new();

    /// <summary>Required by EF Core.</summary>
    protected UserProfile() { }

    private UserProfile(Guid id, string fullName, Country country, DateTime joinedAtUtc)
        : base(id)
    {
        FullName = fullName;
        Country = country;
        JoinedAtUtc = joinedAtUtc;
    }

    // ---------- Profile ----------
    public string FullName { get; private set; } = string.Empty;
    public Country Country { get; private set; }                    // decides the currency
    public string? PhotoUrl { get; private set; }

    // ---------- Notification switches (app's Notifications page) ----------
    public bool NotifyOrderUpdates { get; private set; } = true;
    public bool NotifyShipmentUpdates { get; private set; } = true;
    public bool NotifyOffers { get; private set; }
    public bool NotifyNewDishes { get; private set; } = true;
    public bool NotifyShopOrders { get; private set; } = true;

    // ---------- Lifetime ----------
    public DateTime JoinedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // ---------- Navigation ----------
    public virtual IReadOnlyCollection<DeliveryAddress> Addresses => _addresses.AsReadOnly();
    public DeliveryAddress? DefaultAddress => _addresses.FirstOrDefault(a => a.IsDefault);

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>
    /// Called right after the Identity account is created, with the SAME Id,
    /// so the login account and the profile always match one-to-one.
    /// </summary>
    public static UserProfile Create(Guid identityUserId, string fullName, Country country, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        return new UserProfile(identityUserId, fullName.Trim(), country, utcNow);
    }

    /// <summary>Edit profile page (name and country; phone lives on the login account).</summary>
    public void UpdateProfile(string fullName, Country country)
    {
        EnsureNotDeleted();
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        FullName = fullName.Trim();
        Country = country;
    }

    public void ChangePhoto(string? photoUrl)
    {
        EnsureNotDeleted();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
    }

    public void UpdateNotifications(bool orderUpdates, bool shipmentUpdates, bool offers,
                                    bool newDishes, bool shopOrders)
    {
        NotifyOrderUpdates = orderUpdates;
        NotifyShipmentUpdates = shipmentUpdates;
        NotifyOffers = offers;
        NotifyNewDishes = newDishes;
        NotifyShopOrders = shopOrders;
    }

    // ---------- Addresses ----------

    /// <summary>Add a delivery address. The first one becomes the default automatically.</summary>
    public DeliveryAddress AddAddress(string label, string fullAddress, Country country, bool makeDefault = false)
    {
        EnsureNotDeleted();
        var address = new DeliveryAddress(Id, label, fullAddress, country);
        _addresses.Add(address);

        if (makeDefault || _addresses.Count == 1)
            SetDefaultAddress(address.Id);

        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = FindAddress(addressId);
        _addresses.Remove(address);

        // Keep exactly one default while any address is left
        if (address.IsDefault && _addresses.Count > 0)
            _addresses[0].MarkDefault();
    }

    public void SetDefaultAddress(Guid addressId)
    {
        var chosen = FindAddress(addressId);
        foreach (var a in _addresses) a.ClearDefault();
        chosen.MarkDefault();
    }

    // ---------- Delete account ----------

    /// <summary>
    /// Soft delete: personal details are wiped, but the row stays so past orders still
    /// point at a valid customer. The Application layer also deletes / anonymises the
    /// Identity login and closes the user's shop.
    /// </summary>
    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAtUtc = utcNow;
        FullName = "Deleted user";
        PhotoUrl = null;
        _addresses.Clear();
    }

    private DeliveryAddress FindAddress(Guid addressId) =>
        _addresses.FirstOrDefault(a => a.Id == addressId)
        ?? throw new InvalidOperationException("That address wasn't found on this account.");

    private void EnsureNotDeleted()
    {
        if (IsDeleted) throw new InvalidOperationException("This account has been deleted.");
    }
}