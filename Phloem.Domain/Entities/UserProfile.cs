using Phloem.Domain.Enums;
using Phloem.Domain.Errors;
using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Entities;

/// <summary>
/// The business side of a Phloem account: name, country, photo, delivery addresses,
/// notification switches. It has the SAME Id as the login account (ApplicationUser in
/// Persistence), which owns email, phone, password and roles.
/// "Member since" = CreatedAt (from the base Entity).
///
/// Errors: business-rule failures are RETURNED as a Result (see UserProfileErrors);
/// programmer mistakes (blank names / addresses the validator should have caught) still THROW.
/// </summary>
public class UserProfile : Entity<Guid>
{
    // ---------- Limits (used by validators in Application and column sizes in Persistence) ----------
    public const int FullNameMaxLength = 100;

    private readonly List<DeliveryAddress> _addresses = new();

    /// <summary>Required by EF Core.</summary>
    protected UserProfile() { }

    private UserProfile(Guid id, string fullName, Country country)
        : base(id)
    {
        FullName = fullName;
        Country = country;
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

    // ---------- Navigation ----------
    public virtual IReadOnlyCollection<DeliveryAddress> Addresses => _addresses.AsReadOnly();
    public DeliveryAddress? DefaultAddress => _addresses.FirstOrDefault(a => a.IsDefault);

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>
    /// Called right after the Identity account is created, with the SAME Id,
    /// so the login account and the profile always match one-to-one.
    /// Can't fail for business reasons, so it returns the profile directly.
    /// </summary>
    public static UserProfile Create(Guid identityUserId, string fullName, Country country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fullName.Trim().Length, FullNameMaxLength, nameof(fullName));

        return new UserProfile(identityUserId, fullName.Trim(), country);
    }

    /// <summary>Edit profile page (name and country; phone lives on the login account).</summary>
    public Result UpdateProfile(string fullName, Country country)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fullName.Trim().Length, FullNameMaxLength, nameof(fullName));
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        FullName = fullName.Trim();
        Country = country;
        return Result.Success();
    }

    /// <summary>Set or clear the profile photo (null / blank = remove).</summary>
    public Result ChangePhoto(string? photoUrl)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        return Result.Success();
    }

    public Result UpdateNotifications(bool orderUpdates, bool shipmentUpdates, bool offers,
                                      bool newDishes, bool shopOrders)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        NotifyOrderUpdates = orderUpdates;
        NotifyShipmentUpdates = shipmentUpdates;
        NotifyOffers = offers;
        NotifyNewDishes = newDishes;
        NotifyShopOrders = shopOrders;
        return Result.Success();
    }

    // ---------- Addresses ----------

    /// <summary>Add a delivery address. The first one becomes the default automatically.</summary>
    public Result<DeliveryAddress> AddAddress(string label, string fullAddress, Country country, bool makeDefault = false)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        var address = new DeliveryAddress(Id, label, fullAddress, country);
        _addresses.Add(address);

        if (makeDefault || _addresses.Count == 1)
            MakeDefault(address);

        return address;
    }

    /// <summary>
    /// Edit a saved address. Goes through the profile (not DeliveryAddress directly)
    /// so nobody can change another account's address or a deleted account's address.
    /// </summary>
    public Result UpdateAddress(Guid addressId, string label, string fullAddress, Country country)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        var address = FindAddress(addressId);
        if (address is null) return UserProfileErrors.AddressNotFound;

        address.Update(label, fullAddress, country);
        return Result.Success();
    }

    public Result RemoveAddress(Guid addressId)
    {
        var address = FindAddress(addressId);
        if (address is null) return UserProfileErrors.AddressNotFound;

        _addresses.Remove(address);

        // Keep exactly one default while any address is left
        if (address.IsDefault && _addresses.Count > 0)
            _addresses[0].MarkDefault();

        return Result.Success();
    }

    public Result SetDefaultAddress(Guid addressId)
    {
        var address = FindAddress(addressId);
        if (address is null) return UserProfileErrors.AddressNotFound;

        MakeDefault(address);
        return Result.Success();
    }

    // ---------- Delete account ----------

    /// <summary>
    /// Soft delete: personal details are wiped, but the row stays so past orders still
    /// point at a valid customer. The Application layer also deletes / anonymises the
    /// Identity login and closes the user's shop. Deleting twice is fine.
    /// </summary>
    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        FullName = "Deleted user";
        PhotoUrl = null;
        _addresses.Clear();

        SoftDelete(utcNow);   // base Entity: sets IsDeleted + DeletedAt
    }

    // ---------- Helpers ----------

    private DeliveryAddress? FindAddress(Guid addressId) =>
        _addresses.FirstOrDefault(a => a.Id == addressId);

    private void MakeDefault(DeliveryAddress chosen)
    {
        foreach (var a in _addresses) a.ClearDefault();
        chosen.MarkDefault();
    }
}