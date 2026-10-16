using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The price range an admin allows for one dish name in one country (min and max INCLUSIVE),
/// e.g. Jollof Rice in Ghana: GH₵ 35–60. Sellers choose their own price inside it.
/// Belongs to a DishCatalog and is only created / changed through it.
/// </summary>
public class DishPriceRange : Entity<Guid>
{
    protected DishPriceRange() { }

    internal DishPriceRange(Guid dishCatalogId, Country country, decimal minPrice, decimal maxPrice)
        : base(Guid.CreateVersion7())
    {
        DishCatalogId = dishCatalogId;
        Country = country;
        Set(minPrice, maxPrice);
    }

    public Guid DishCatalogId { get; private set; }
    public Country Country { get; private set; }
    public Currency Currency => Country.ToCurrency();

    public decimal MinPrice { get; private set; }
    public decimal MaxPrice { get; private set; }

    /// <summary>Inclusive: both MinPrice and MaxPrice are allowed.</summary>
    public bool Contains(decimal price) => price >= MinPrice && price <= MaxPrice;

    internal void Update(decimal minPrice, decimal maxPrice) => Set(minPrice, maxPrice);

    private void Set(decimal minPrice, decimal maxPrice)
    {
        // The validator checks these first, so getting here with bad values is a bug
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minPrice);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPrice, minPrice);

        MinPrice = decimal.Round(minPrice, 2, MidpointRounding.AwayFromZero);
        MaxPrice = decimal.Round(maxPrice, 2, MidpointRounding.AwayFromZero);
    }
}