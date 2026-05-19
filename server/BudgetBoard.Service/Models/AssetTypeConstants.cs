namespace BudgetBoard.Service.Models;

public static class AssetTypeConstants
{
    public static readonly IEnumerable<IAssetType> DefaultAssetTypes = new List<IAssetType>(
        [
            // Real Estate
            new AssetTypeBase { Value = "Real Estate", Parent = "" },
            new AssetTypeBase { Value = "Primary Residence", Parent = "Real Estate" },
            new AssetTypeBase { Value = "Rental Property", Parent = "Real Estate" },
            new AssetTypeBase { Value = "Vacation Home", Parent = "Real Estate" },
            new AssetTypeBase { Value = "Land", Parent = "Real Estate" },
            new AssetTypeBase { Value = "Commercial Property", Parent = "Real Estate" },
            // Vehicle
            new AssetTypeBase { Value = "Vehicle", Parent = "" },
            new AssetTypeBase { Value = "Automobile", Parent = "Vehicle" },
            new AssetTypeBase { Value = "Motorcycle", Parent = "Vehicle" },
            new AssetTypeBase { Value = "Boat", Parent = "Vehicle" },
            new AssetTypeBase { Value = "RV", Parent = "Vehicle" },
            new AssetTypeBase { Value = "Aircraft", Parent = "Vehicle" },
            // Valuables
            new AssetTypeBase { Value = "Valuables", Parent = "" },
            new AssetTypeBase { Value = "Jewelry", Parent = "Valuables" },
            new AssetTypeBase { Value = "Art", Parent = "Valuables" },
            new AssetTypeBase { Value = "Collectibles", Parent = "Valuables" },
            new AssetTypeBase { Value = "Watches", Parent = "Valuables" },
            // Personal Property
            new AssetTypeBase { Value = "Personal Property", Parent = "" },
            new AssetTypeBase { Value = "Electronics", Parent = "Personal Property" },
            new AssetTypeBase { Value = "Furniture & Appliances", Parent = "Personal Property" },
            new AssetTypeBase { Value = "Musical Instruments", Parent = "Personal Property" },
            new AssetTypeBase { Value = "Tools & Equipment", Parent = "Personal Property" },
        ]
    );
}
