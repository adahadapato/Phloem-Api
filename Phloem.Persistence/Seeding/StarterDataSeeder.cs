using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// Seeds the starter data from <c>starter-data.json</c>, one block at a time:
/// <list type="number">
///   <item><description>Commissions</description></item>
///   <item><description>Delivery pricing (with window charges)</description></item>
///   <item><description>Dish names (with price ranges)</description></item>
///   <item><description>Shipping routes (switched off, no rates)</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para><b>Each block runs only if its table is completely empty</b> (deleted rows included), so anything an admin
/// changes or removes is never brought back by a restart.</para>
/// <para>Everything goes through the Domain's own methods, so the same rules apply as in the app. A bad value in the
/// file stops start-up with a message naming the entry.</para>
/// </remarks>
internal static class StarterDataSeeder
{
    /// <summary>Reads enum names ("Ghana", "TodayEvening") and any letter case.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Seeds whatever is missing.</summary>
    /// <param name="context">The database.</param>
    /// <param name="settings">The <c>SeedData</c> section.</param>
    /// <param name="logger">Start-up log.</param>
    /// <param name="cancellationToken">Stops seeding.</param>
    public static async Task SeedAsync(PhloemDbContext context, SeedDataSettings settings, ILogger logger,
                                       CancellationToken cancellationToken = default)
    {
        if (!settings.StarterDataEnabled) return;

        var data = await ReadAsync(settings.StarterDataPath, logger, cancellationToken);
        if (data is null) return;

        var seeded = new List<string>();

        // 1. Commissions
        if (!await context.Commissions.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            foreach (var c in data.Commissions)
                context.Commissions.Add(Run($"commission {c.Country}", () => Commission.Create(c.Country, c.RatePercent)));
            seeded.Add($"{data.Commissions.Count} commissions");
        }

        // 2. Delivery pricing
        if (!await context.DeliveryPricings.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            foreach (var p in data.DeliveryPricing)
            {
                var pricing = Run($"delivery pricing {p.Country}",
                                  () => DeliveryPricing.Create(p.Country, p.MinShopFee, p.MaxShopFee));
                foreach (var w in p.Windows)
                    Run($"delivery window {p.Country}/{w.Window}", () => pricing.SetWindowCharge(w.Window, w.ExtraCharge));

                context.DeliveryPricings.Add(pricing);
            }
            seeded.Add($"{data.DeliveryPricing.Count} delivery pricings");
        }

        // 3. Dish names
        if (!await context.DishCatalogs.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            foreach (var d in data.Dishes)
            {
                var dish = Run($"dish '{d.Name}'", () => DishCatalog.Create(d.Name, d.Category, d.Group));
                foreach (var r in d.PriceRanges)
                {
                    var range = dish.SetPriceRange(r.Country, r.Min, r.Max);
                    if (range.IsFailure)
                        throw new InvalidOperationException(
                            $"starter-data.json: price range for '{d.Name}' in {r.Country} is invalid: {range.Error.Message}");
                }

                context.DishCatalogs.Add(dish);
            }
            seeded.Add($"{data.Dishes.Count} dishes");
        }

        // 4. Shipping routes (off until an admin adds a real drop-off point and rates)
        if (!await context.ShipmentRoutes.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            foreach (var r in data.ShipmentRoutes)
                context.ShipmentRoutes.Add(Run($"route {r.From}→{r.To}",
                    () => ShipmentRoute.Create(r.From, r.To, r.DropOffAddress, r.DropOffHours)));
            seeded.Add($"{data.ShipmentRoutes.Count} shipping routes");
        }

        if (seeded.Count == 0) return;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded starter data: {Blocks}", string.Join(", ", seeded));
    }

    /// <summary>Finds and reads the JSON file; null (with a warning) if it's missing.</summary>
    private static async Task<StarterData?> ReadAsync(string path, ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogWarning("Starter data skipped: SeedData:StarterDataPath is empty.");
            return null;
        }

        var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(fullPath))
        {
            logger.LogWarning("Starter data skipped: {Path} not found.", fullPath);
            return null;
        }

        await using var stream = File.OpenRead(fullPath);
        return await JsonSerializer.DeserializeAsync<StarterData>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"{fullPath} is empty.");
    }

    /// <summary>Runs a Domain call; a guard failure becomes a message naming the entry in the file.</summary>
    private static T Run<T>(string entry, Func<T> create)
    {
        try { return create(); }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"starter-data.json: {entry} is invalid: {ex.Message}", ex);
        }
    }

    /// <summary>Same as <see cref="Run{T}"/>, for calls that return nothing.</summary>
    private static void Run(string entry, Action change) => Run(entry, () => { change(); return true; });
}