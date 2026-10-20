using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;
using Phloem.Persistence.Configurations.Identity;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// Seeds the DEMO shops from <c>demo-data.json</c> so the app has shops and dishes to show: each shop with a real
/// (demo) owner account, an approved shop with logo, cover photo, delivery fee and ratings, and its dishes with photos.
/// </summary>
/// <remarks>
/// <para><b>Only when <c>SeedData:DemoDataEnabled</c> is true.</b> Turn it off before launch.</para>
/// <para><b>Once per shop:</b> a shop is skipped when its owner's email already exists, so restarts and deploys never
/// duplicate it, and a demo shop an admin blocked stays blocked. It's also skipped (with a warning) when a shop with
/// the same name already exists, because shop names are unique.</para>
/// <para><b>One transaction per shop:</b> owner, shop and dishes are all created, or none.</para>
/// <para><b>Pictures:</b> ready-made WebP files from <c>demo-photos/</c>, copied into storage as they are. They skip
/// the upload checks (size, blur): real sellers' photos always go through them.</para>
/// <para><b>Owners can sign in</b> with <c>SeedData:DemoOwnerPassword</c> (a secret) to try the seller screens.
/// When it's empty, each demo account gets a random password nobody knows.</para>
/// <para>Runs after the starter data: dish names, price ranges and delivery pricing must exist.</para>
/// </remarks>
internal static class DemoDataSeeder
{
    private const string WebP = "image/webp";

    /// <summary>Reads enum names ("UnitedKingdom"), comments and any letter case.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Seeds the demo shops that aren't there yet.</summary>
    /// <param name="userManager">Identity's user manager (same DbContext as <paramref name="context"/>).</param>
    /// <param name="context">The database.</param>
    /// <param name="storage">Where the pictures go (public container).</param>
    /// <param name="settings">The <c>SeedData</c> section.</param>
    /// <param name="time">The clock.</param>
    /// <param name="logger">Start-up log.</param>
    /// <param name="cancellationToken">Stops seeding.</param>
    /// <exception cref="InvalidOperationException">A value in demo-data.json breaks a rule (message names the entry).</exception>
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, PhloemDbContext context,
                                       IFileStorage storage, SeedDataSettings settings, TimeProvider time,
                                       ILogger logger, CancellationToken cancellationToken = default)
    {
        if (!settings.DemoDataEnabled) return;

        var (data, folder) = await ReadAsync(settings.DemoDataPath, logger, cancellationToken);
        if (data is null) return;

        var password = string.IsNullOrWhiteSpace(settings.DemoOwnerPassword)
            ? RandomPassword()
            : settings.DemoOwnerPassword;

        var added = new List<string>();
        foreach (var shop in data.Shops)
        {
            // Already seeded (its demo owner exists): nothing to do
            if (await userManager.FindByEmailAsync(shop.Owner.Email.Trim()) is not null) continue;

            // A shop with this name already exists (a real or test shop): shop names are unique, so leave it alone
            var name = shop.Name.Trim();
            if (await context.Vendors.IgnoreQueryFilters().AnyAsync(v => v.Name == name, cancellationToken))
            {
                logger.LogWarning("Demo shop '{Shop}' skipped: a shop with that name already exists.", name);
                continue;
            }

            await SeedShopAsync(shop, folder, password, userManager, context, storage, time, cancellationToken);
            added.Add(shop.Name);
        }

        if (added.Count > 0)
            logger.LogInformation("Seeded demo shops: {Shops}", string.Join(", ", added));
    }

    /// <summary>Owner + shop + dishes for one demo shop, in one transaction.</summary>
    private static async Task SeedShopAsync(DemoShop shop, string folder, string password,
                                            UserManager<ApplicationUser> userManager, PhloemDbContext context,
                                            IFileStorage storage, TimeProvider time, CancellationToken ct)
    {
        var entry = $"shop '{shop.Name}'";
        var now = time.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);

        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        // 1. Owner: login account (Customer + Seller) and profile with personal details
        var owner = shop.Owner;
        var user = new ApplicationUser
        {
            UserName = owner.Email.Trim(),
            Email = owner.Email.Trim(),
            EmailConfirmed = true,
            PhoneNumber = owner.Phone
        };
        IdentityResultCheck.Ensure(await userManager.CreateAsync(user, password), $"create demo owner '{owner.Email}'");
        IdentityResultCheck.Ensure(await userManager.AddToRoleAsync(user, Roles.Customer), $"give '{owner.Email}' Customer");
        IdentityResultCheck.Ensure(await userManager.AddToRoleAsync(user, Roles.Seller), $"give '{owner.Email}' Seller");

        var profile = Run(entry, () => UserProfile.Create(user.Id, owner.FirstName, owner.LastName, owner.Country));
        var address = Run(entry, () => ResidentialAddress.Create(owner.Address.Line1, owner.Address.Line2,
                                                                 owner.Address.City, owner.Address.RegionOrPostcode,
                                                                 owner.Country));
        Check(entry, profile.UpdatePersonalDetails(owner.DateOfBirth, address, today));
        context.UserProfiles.Add(profile);

        // 2. Shop: opened, approved, delivery fee, ratings
        var phone = string.IsNullOrWhiteSpace(owner.Phone) ? null : Run(entry, () => PhoneNumber.Create(owner.Phone));
        var vendor = Check(entry, Run(entry, () => Vendor.Open(profile, shop.Name,
                                                               EmailAddress.Create(shop.BusinessEmail), phone)));
        Check(entry, vendor.Approve(now));

        var pricing = await context.DeliveryPricings.FirstOrDefaultAsync(p => p.Country == owner.Country, ct)
            ?? throw new InvalidOperationException($"demo-data.json: {entry} needs delivery pricing for {owner.Country} " +
                                                   "(starter data).");
        Check(entry, vendor.SetDeliveryFee(shop.DeliveryFee, pricing));
        foreach (var stars in shop.Ratings) vendor.AddRating(stars);

        // 3. Pictures
        vendor.ChangeLogo(await SaveAsync(storage, folder, shop.Logo.File, null,
                                          ImageDimensions.Create(shop.Logo.Width, shop.Logo.Height),
                                          $"vendors/{vendor.Id:N}/logo-{Guid.CreateVersion7():N}.webp", null, ct));
        vendor.ChangeCoverPhoto(await SaveAsync(storage, folder, shop.CoverPhoto.File, null,
                                                ImageDimensions.Create(shop.CoverPhoto.Width, shop.CoverPhoto.Height),
                                                $"vendors/{vendor.Id:N}/cover-photo-{Guid.CreateVersion7():N}.webp",
                                                null, ct));
        context.Vendors.Add(vendor);

        // 4. Dishes, each with its photo and ratings
        foreach (var d in shop.Dishes)
        {
            var dishEntry = $"{entry}, dish '{d.Name}'";
            var catalog = await context.DishCatalogs.Include(c => c.PriceRanges)
                                       .FirstOrDefaultAsync(c => c.Name == d.Name, ct)
                ?? throw new InvalidOperationException($"demo-data.json: {dishEntry} isn't in the dish catalog.");

            var dish = Check(dishEntry, Dish.Create(vendor, catalog, d.Detail, d.Price, d.Description));
            dish.SetAvailability(true);

            var name = Guid.CreateVersion7().ToString("N");

            // A NEW ImageDimensions for every photo: EF Core stores it as part of the photo's row, and one object
            // shared by several photos (like the DishPhotoStandard.FullSize constant) is saved for only one of them
            var size = ImageDimensions.Create(DishPhotoStandard.FullSize.Width, DishPhotoStandard.FullSize.Height);
            var photo = await SaveAsync(storage, folder, $"{d.Photo}.webp", $"{d.Photo}_thumb.webp",
                                        size, $"dishes/{dish.Id:N}/{name}.webp",
                                        $"dishes/{dish.Id:N}/{name}_thumb.webp", ct);
            Check(dishEntry, dish.AddPhoto(photo, dish.Name));

            foreach (var stars in d.Ratings) dish.AddRating(stars);
            context.Dishes.Add(dish);
        }

        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    /// <summary>Copies a ready-made picture (and its thumbnail) into public storage.</summary>
    private static async Task<ImageFile> SaveAsync(IFileStorage storage, string folder, string file, string? thumbFile,
                                                   ImageDimensions dimensions, string path, string? thumbPath,
                                                   CancellationToken ct)
    {
        var bytes = await File.ReadAllBytesAsync(Source(folder, file), ct);
        await using (var stream = new MemoryStream(bytes))
            await storage.SaveAsync(stream, path, WebP, StorageContainer.Public, ct);

        if (thumbFile is not null && thumbPath is not null)
        {
            await using var thumb = File.OpenRead(Source(folder, thumbFile));
            await storage.SaveAsync(thumb, thumbPath, WebP, StorageContainer.Public, ct);
        }

        return ImageFile.Create(path, thumbFile is null ? null : thumbPath, WebP, bytes.Length, dimensions,
                                Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>A picture in demo-photos/; stops start-up with a clear message when it's missing.</summary>
    private static string Source(string folder, string file)
    {
        var full = Path.Combine(folder, "demo-photos", file.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(full)
            ? full
            : throw new InvalidOperationException($"demo-data.json: picture '{file}' not found at {full}.");
    }

    /// <summary>Finds and reads the JSON file; null (with a warning) if it's missing.</summary>
    /// <returns>The data and the folder it's in (the pictures are next to it).</returns>
    private static async Task<(DemoData? Data, string Folder)> ReadAsync(string path, ILogger logger,
                                                                         CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogWarning("Demo data skipped: SeedData:DemoDataPath is empty.");
            return (null, string.Empty);
        }

        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(fullPath))
        {
            logger.LogWarning("Demo data skipped: {Path} not found.", fullPath);
            return (null, string.Empty);
        }

        await using var stream = File.OpenRead(fullPath);
        var data = await JsonSerializer.DeserializeAsync<DemoData>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"{fullPath} is empty.");
        return (data, Path.GetDirectoryName(fullPath)!);
    }

    /// <summary>A random password that meets the password rules (used when no demo password is set).</summary>
    private static string RandomPassword() =>
        $"Demo-{Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))}-a1A";

    /// <summary>A failed Domain result stops start-up with a message naming the entry.</summary>
    private static void Check(string entry, Result result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"demo-data.json: {entry} is invalid: {result.Error.Message}");
    }

    /// <summary>Same as <see cref="Check(string, Result)"/>, returning the value.</summary>
    private static T Check<T>(string entry, Result<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"demo-data.json: {entry} is invalid: {result.Error.Message}");
        return result.Value;
    }

    /// <summary>Runs a Domain call; a guard failure becomes a message naming the entry.</summary>
    private static T Run<T>(string entry, Func<T> create)
    {
        try { return create(); }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"demo-data.json: {entry} is invalid: {ex.Message}", ex);
        }
    }
}
