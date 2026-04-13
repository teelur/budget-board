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
    DateOnly? PurchaseDate { get; }
    decimal? PurchasePrice { get; }
    DateOnly? SellDate { get; }
    decimal? SellPrice { get; }
    bool Hide { get; }
    DateTime? Deleted { get; }
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
    public int Index { get; set; } = 0;
    public Guid UserID { get; set; } = Guid.Empty;

    public AssetResponse(Asset asset)
        : this()
    {
        ID = asset.ID;
        Name = asset.Name;
        CurrentValue = asset
            .Values.OrderByDescending(v => v.Date)
            .FirstOrDefault(v => v.Deleted == null)
            ?.Amount;
        ValueDate = asset
            .Values.OrderByDescending(v => v.Date)
            .FirstOrDefault(v => v.Deleted == null)
            ?.Date;
        PurchaseDate = asset.PurchaseDate;
        PurchasePrice = asset.PurchasePrice;
        SellDate = asset.SellDate;
        SellPrice = asset.SellPrice;
        Hide = asset.Hide;
        Deleted = asset.Deleted;
        Index = asset.Index;
        UserID = asset.UserID;
    }
}

public interface IAssetUpdateRequest
{
    Guid ID { get; }
    string Name { get; }
    DateOnly? PurchaseDate { get; }
    decimal? PurchasePrice { get; }
    DateOnly? SellDate { get; }
    decimal? SellPrice { get; }
    bool Hide { get; }
}

public class AssetUpdateRequest() : IAssetUpdateRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly? PurchaseDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateOnly? SellDate { get; set; } = null;
    public decimal? SellPrice { get; set; } = null;
    public bool Hide { get; set; } = false;
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

    public AssetIndexRequest(Asset asset)
        : this()
    {
        ID = asset.ID;
        Index = asset.Index;
    }
}
