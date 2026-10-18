using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// The business side of a Phloem account: name, country, photo, personal details, delivery addresses, notification
/// switches. It has the SAME Id as the login account (ApplicationUser in Persistence), which owns email, phone,
/// password and roles. "Member since" = CreatedAt (from the base Entity).
/// </summary>
/// <remarks>
/// <para><b>Names:</b> first and last name are stored separately (the app greets people by first name);
/// <see cref="GetFullName"/> joins them wherever the whole name is shown.</para>
/// <para><b>Personal details</b> (date of birth + home address) are optional, but required, and the user must be
/// <see cref="MinimumTradingAge"/> or older, before opening a shop or sending a parcel (<see cref="CheckCanTrade"/>).</para>
/// <para><b>Home address vs delivery addresses:</b> one <see cref="ResidentialAddress"/> says where the person lives;
/// <see cref="Addresses"/> are places food can be delivered to (any number).</para>
/// <para><b>Errors:</b> business-rule failures are RETURNED as a Result (see UserProfileErrors); programmer mistakes
/// (blank names / addresses the validator should have caught) still THROW.</para>
/// </remarks>
public class UserProfile : Entity<Guid>
{
    // ---------- Limits (validators in Application, column sizes in Persistence) ----------

    /// <summary>Longest first name.</summary>
    public const int FirstNameMaxLength = 50;

    /// <summary>Longest last name.</summary>
    public const int LastNameMaxLength = 50;

    /// <summary>Longest full name (first + space + last): the size of name snapshots, e.g. a shipment's sender.</summary>
    public const int FullNameMaxLength = FirstNameMaxLength + 1 + LastNameMaxLength;

    /// <summary>Age needed to open a shop or send a parcel (a contract and legal rule).</summary>
    public const int MinimumTradingAge = 18;

    /// <summary>Oldest believable age; anything older is a typing mistake.</summary>
    public const int MaximumAge = 120;

    private readonly List<DeliveryAddress> _addresses = new();

    /// <summary>Required by EF Core.</summary>
    protected UserProfile() { }

    private UserProfile(Guid id, string firstName, string lastName, Country country)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Country = country;
    }

    // ---------- Profile ----------

    /// <summary>First (given) name, e.g. "Kofi".</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Last (family) name, e.g. "Boateng".</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>The country the user shops in; decides the currency.</summary>
    public Country Country { get; private set; }

    /// <summary>Storage path of the profile photo; null = no photo.</summary>
    public string? PhotoUrl { get; private set; }

    // ---------- Personal details ----------

    /// <summary>Date of birth; null until the user adds it.</summary>
    public DateOnly? DateOfBirth { get; private set; }

    /// <summary>Where the user lives; null until the user adds it.</summary>
    public ResidentialAddress? ResidentialAddress { get; private set; }

    /// <summary>True once both date of birth and home address are filled in.</summary>
    public bool HasPersonalDetails => DateOfBirth is not null && ResidentialAddress is not null;

    // ---------- Notification switches (app's Notifications page) ----------

    /// <summary>Emails about the user's orders (incl. the weekly price-change email).</summary>
    public bool NotifyOrderUpdates { get; private set; } = true;

    /// <summary>Emails about the user's parcels.</summary>
    public bool NotifyShipmentUpdates { get; private set; } = true;

    /// <summary>Promotional offers (off by default).</summary>
    public bool NotifyOffers { get; private set; }

    /// <summary>New dishes from followed shops.</summary>
    public bool NotifyNewDishes { get; private set; } = true;

    /// <summary>Sellers: new orders for their shop.</summary>
    public bool NotifyShopOrders { get; private set; } = true;

    // ---------- Navigation ----------

    /// <summary>Saved delivery addresses.</summary>
    public virtual IReadOnlyCollection<DeliveryAddress> Addresses => _addresses.AsReadOnly();

    /// <summary>The default address, if any.</summary>
    public DeliveryAddress? DefaultAddress => _addresses.FirstOrDefault(a => a.IsDefault);

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>"Kofi Boateng": first and last name joined by one space.</summary>
    /// <returns>The full name.</returns>
    public string GetFullName() => $"{FirstName} {LastName}";

    /// <summary>The user's age in whole years on a given day; null without a date of birth.</summary>
    /// <param name="today">Today's date.</param>
    public int? AgeOn(DateOnly today) => DateOfBirth is { } born ? YearsBetween(born, today) : null;

    /// <summary>
    /// Called right after the Identity account is created, with the SAME Id, so the login account and the profile
    /// always match one-to-one. Can't fail for business reasons, so it returns the profile directly.
    /// </summary>
    /// <param name="identityUserId">The new login account's Id.</param>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    /// <param name="country">Country the user shops in.</param>
    /// <returns>The new profile.</returns>
    /// <exception cref="ArgumentException">A blank name.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A name longer than its limit.</exception>
    public static UserProfile Create(Guid identityUserId, string firstName, string lastName, Country country)
    {
        var (first, last) = CheckNames(firstName, lastName);
        return new UserProfile(identityUserId, first, last, country);
    }

    /// <summary>Edit profile page (names and country; phone lives on the login account).</summary>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    /// <param name="country">Country the user shops in.</param>
    /// <returns>Success; or <see cref="UserProfileErrors.AccountDeleted"/>.</returns>
    /// <exception cref="ArgumentException">A blank name.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A name longer than its limit.</exception>
    public Result UpdateProfile(string firstName, string lastName, Country country)
    {
        var (first, last) = CheckNames(firstName, lastName);
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        FirstName = first;
        LastName = last;
        Country = country;
        return Result.Success();
    }

    /// <summary>Set or clear the profile photo (null / blank = remove).</summary>
    /// <param name="photoUrl">The photo's storage path.</param>
    /// <returns>Success; or <see cref="UserProfileErrors.AccountDeleted"/>.</returns>
    public Result ChangePhoto(string? photoUrl)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
        return Result.Success();
    }

    /// <summary>Notifications page.</summary>
    /// <returns>Success; or <see cref="UserProfileErrors.AccountDeleted"/>.</returns>
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

    // ---------- Personal details ----------

    /// <summary>Sets (or changes) date of birth and home address together.</summary>
    /// <param name="dateOfBirth">Date of birth.</param>
    /// <param name="residentialAddress">Where the user lives.</param>
    /// <param name="today">Today's date (from the app's clock).</param>
    /// <returns>Success; or <see cref="UserProfileErrors.AccountDeleted"/>,
    /// <see cref="UserProfileErrors.InvalidDateOfBirth"/>.</returns>
    /// <remarks>Under-18s may save their details (they can still order food); <see cref="CheckCanTrade"/> decides
    /// what they're allowed to do.</remarks>
    public Result UpdatePersonalDetails(DateOnly dateOfBirth, ResidentialAddress residentialAddress, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(residentialAddress);
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        if (dateOfBirth > today || YearsBetween(dateOfBirth, today) > MaximumAge)
            return UserProfileErrors.InvalidDateOfBirth;

        DateOfBirth = dateOfBirth;
        ResidentialAddress = residentialAddress;
        return Result.Success();
    }

    /// <summary>
    /// Before opening a shop or sending a parcel: personal details must be filled in, and the user must be
    /// <see cref="MinimumTradingAge"/> or older.
    /// </summary>
    /// <param name="today">Today's date (from the app's clock).</param>
    /// <returns>Success; or <see cref="UserProfileErrors.AccountDeleted"/>,
    /// <see cref="UserProfileErrors.PersonalDetailsRequired"/>, <see cref="UserProfileErrors.TooYoung"/>.</returns>
    public Result CheckCanTrade(DateOnly today)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;
        if (!HasPersonalDetails) return UserProfileErrors.PersonalDetailsRequired;
        if (AgeOn(today) < MinimumTradingAge) return UserProfileErrors.TooYoung;
        return Result.Success();
    }

    // ---------- Addresses ----------

    /// <summary>Add a delivery address. The first one becomes the default automatically.</summary>
    /// <returns>The new address; or <see cref="UserProfileErrors.AccountDeleted"/>.</returns>
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
    /// Edit a saved address. Goes through the profile (not DeliveryAddress directly) so nobody can change another
    /// account's address or a deleted account's address.
    /// </summary>
    /// <returns>Success; or <see cref="UserProfileErrors.AccountDeleted"/>, <see cref="UserProfileErrors.AddressNotFound"/>.</returns>
    public Result UpdateAddress(Guid addressId, string label, string fullAddress, Country country)
    {
        if (IsDeleted) return UserProfileErrors.AccountDeleted;

        var address = FindAddress(addressId);
        if (address is null) return UserProfileErrors.AddressNotFound;

        address.Update(label, fullAddress, country);
        return Result.Success();
    }

    /// <summary>Remove a saved address; if it was the default, the next one becomes default.</summary>
    /// <returns>Success; or <see cref="UserProfileErrors.AddressNotFound"/>.</returns>
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

    /// <summary>Make one saved address the default.</summary>
    /// <returns>Success; or <see cref="UserProfileErrors.AddressNotFound"/>.</returns>
    public Result SetDefaultAddress(Guid addressId)
    {
        var address = FindAddress(addressId);
        if (address is null) return UserProfileErrors.AddressNotFound;

        MakeDefault(address);
        return Result.Success();
    }

    // ---------- Delete account ----------

    /// <summary>
    /// Soft delete: personal details are wiped, but the row stays so past orders still point at a valid customer.
    /// The Application layer also deletes / anonymises the Identity login and closes the user's shop. Deleting twice
    /// is fine.
    /// </summary>
    /// <param name="utcNow">The current time.</param>
    public void Delete(DateTime utcNow)
    {
        if (IsDeleted) return;

        FirstName = "Deleted";
        LastName = "user";
        PhotoUrl = null;
        DateOfBirth = null;
        ResidentialAddress = null;
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

    /// <summary>Whole years from one date to another (birthdays not yet reached this year don't count).</summary>
    private static int YearsBetween(DateOnly from, DateOnly to)
    {
        var years = to.Year - from.Year;
        if (from > to.AddYears(-years)) years--;
        return years;
    }

    /// <summary>Trims and checks both names.</summary>
    /// <exception cref="ArgumentException">A blank name.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A name longer than its limit.</exception>
    private static (string First, string Last) CheckNames(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        var first = firstName.Trim();
        var last = lastName.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(first.Length, FirstNameMaxLength, nameof(firstName));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(last.Length, LastNameMaxLength, nameof(lastName));
        return (first, last);
    }
}