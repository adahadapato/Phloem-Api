namespace Phloem.Domain.Enums;

/// <summary>Documents a seller can upload for the admin to check.</summary>
public enum VendorDocumentType
{
    BusinessRegistration = 1,     // proof the business is registered
    FoodHygieneCertificate = 2,   // food safety / hygiene permit or certificate
    OwnerId = 3,                  // owner's photo ID
    Other = 4
}