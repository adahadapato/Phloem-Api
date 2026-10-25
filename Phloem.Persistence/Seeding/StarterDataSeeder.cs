using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Persistence.Seeding;

/// <summary>
/// Seeds the starter data from <c>starter-data.json</c>, one block at a time:
/// <list type="number">
///   <item><description>Commissions</description></item>
///   <item><description>Delivery pricing (with window charges)</description></item>
///   <item><description>Dish names (with price ranges)</description></item>
///   <item><description>Shipping routes (switched off, no rates)</description></item>
///   <item><description>Legal texts: the temporary Terms and Privacy Policy (version 1, marked as drafts), from
///   <c>legal/terms.md</c> and <c>legal/privacy.md</c> next to the JSON file. The first line "# Title" is the
///   title; the rest is the text.</description></item>
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

        // 5. Legal texts (temporary drafts; admins publish the reviewed wording as version 2 and on)
        if (!await context.LegalDocuments.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            var dataFolder = Path.GetDirectoryName(FullPath(settings.StarterDataPath)) ?? string.Empty;
            var folder = Path.Combine(dataFolder, "legal");
            var added = 0;
            var drafts = new[] { (LegalDocumentType.Terms, "terms.md"), (LegalDocumentType.Privacy, "privacy.md") };
            foreach (var (type, file) in drafts)
            {
                var draft = await ReadLegalDraftAsync(Path.Combine(folder, file), logger, cancellationToken);
                if (draft is null) continue;
                var (title, content) = draft.Value;
                context.LegalDocuments.Add(Run($"legal/{file}", () =>
                    LegalDocument.Publish(type, 1, title, content, isDraft: true, DateTime.UtcNow)));
                added++;
            }
            if (added > 0) seeded.Add($"{added} legal drafts");
        }

        if (seeded.Count == 0) return;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded starter data: {Blocks}", string.Join(", ", seeded));
    }

    /// <summary>A relative path is read from the app's folder (where the build copies Seeding/Data).</summary>
    private static string FullPath(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);

    /// <summary>Reads a legal draft: the first "# Title" line is the title, the rest the text; null if missing.</summary>
    private static async Task<(string Title, string Content)?> ReadLegalDraftAsync(
        string path, ILogger logger, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            logger.LogWarning("Legal draft skipped: {Path} not found.", path);
            return null;
        }

        var lines = await File.ReadAllLinesAsync(path, cancellationToken);
        var titleLine = Array.FindIndex(lines, l => l.StartsWith("# ", StringComparison.Ordinal));
        if (titleLine < 0)
            throw new InvalidOperationException(
                $"{path}: the first heading must be the title, e.g. \"# Terms & Conditions\".");

        var title = lines[titleLine][2..].Trim();
        var content = string.Join('\n', lines.Skip(titleLine + 1)).Trim();
        return (title, content);
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