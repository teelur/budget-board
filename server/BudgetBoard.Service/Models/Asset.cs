using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IAssetCreateRequest
{
    string Name { get; }
}

public class AssetCreateRequest() : IAssetCreateRequest
{
    public string Name { get; set; } = string.Empty;
}

public interface IAssetResponse
{
    Guid ID { get; }
    string Name { get; }
    decimal? CurrentValue { get; }
    DateOnly? ValueDate { get; }
    DateOnly? PurchaseDate { get; }
    decimal? PurchasePrice { get; }
    DateOnly? SellDate { get; }
    decimal? SellPrice { get; }
    bool Hide { get; }
    DateTime? Deleted { get; }
    string? Type { get; }
    int Index { get; }
    Guid UserID { get; }
}

public class AssetResponse() : IAssetResponse
{
    public Guid ID { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? CurrentValue { get; set; } = null;
    public DateOnly? ValueDate { get; set; } = null;
    public DateOnly? PurchaseDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateOnly? SellDate { get; set; } = null;
    public decimal? SellPrice { get; set; } = null;
    public bool Hide { get; set; } = false;
    public DateTime? Deleted { get; set; } = null;
    public string? Type { get; set; } = null;
    public int Index { get; set; } = 0;
    public Guid UserID { get; set; } = Guid.Empty;

    public AssetResponse(Asset asset)
        : this()
    {
        ID = asset.ID;
        Name = asset.Name;
        CurrentValue = asset.Values.OrderByDescending(v => v.Date).FirstOrDefault()?.Amount;
        ValueDate = asset.Values.OrderByDescending(v => v.Date).FirstOrDefault()?.Date;
        PurchaseDate = asset.PurchaseDate;
        PurchasePrice = asset.PurchasePrice;
        SellDate = asset.SellDate;
        SellPrice = asset.SellPrice;
        Hide = asset.Hide;
        Deleted = asset.Deleted;
        Type = asset.Type;
        Index = asset.Index;
        UserID = asset.UserID;
    }
}

public interface IAssetUpdateRequest
{
    Guid ID { get; }
    OptionalField<string> Name { get; }
    OptionalField<DateOnly?> PurchaseDate { get; }
    OptionalField<decimal?> PurchasePrice { get; }
    OptionalField<DateOnly?> SellDate { get; }
    OptionalField<decimal?> SellPrice { get; }
    OptionalField<bool> Hide { get; }
    OptionalField<string> Type { get; }
}

public class AssetUpdateRequest() : IAssetUpdateRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public OptionalField<string> Name { get; set; }
    public OptionalField<DateOnly?> PurchaseDate { get; set; }
    public OptionalField<decimal?> PurchasePrice { get; set; }
    public OptionalField<DateOnly?> SellDate { get; set; }
    public OptionalField<decimal?> SellPrice { get; set; }
    public OptionalField<bool> Hide { get; set; }
    public OptionalField<string> Type { get; set; }
}

public interface IAssetIndexRequest
{
    Guid ID { get; }
    int Index { get; }
}

public class AssetIndexRequest() : IAssetIndexRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public int Index { get; set; } = 0;
}
