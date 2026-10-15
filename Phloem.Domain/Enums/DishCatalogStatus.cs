namespace Phloem.Domain.Enums;

public enum DishCatalogStatus
{
    Suggested = 1,   // a seller asked for it, waiting for an admin
    Approved = 2,    // in the list every seller picks from
    Rejected = 3
}