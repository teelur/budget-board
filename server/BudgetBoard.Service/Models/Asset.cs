using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IAssetCreateRequest
{
    string Name { get; set; }
}

public class AssetCreateRequest() : IAssetCreateRequest
{
    public string Name { get; set; } = string.Empty;
}

public interface IAssetResponse
{
    Guid ID { get; set; }
    string Name { get; set; }
    decimal? CurrentValue { get; set; }
    DateTime? PurchaseDate { get; set; }
    decimal? PurchasePrice { get; set; }
    DateTime? SellDate { get; set; }
    decimal? SellPrice { get; set; }
    bool Hide { get; set; }
    DateTime? Deleted { get; set; }
    int Index { get; set; }
    Guid UserID { get; set; }
}

public class AssetResponse() : IAssetResponse
{
    public Guid ID { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? CurrentValue { get; set; } = null;
    public DateTime? ValueDate { get; set; } = null;
    public DateTime? PurchaseDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateTime? SellDate { get; set; } = null;
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
        CurrentValue = asset.Values.OrderByDescending(v => v.DateTime).FirstOrDefault()?.Amount;
        ValueDate = asset.Values.OrderByDescending(v => v.DateTime).FirstOrDefault()?.DateTime;
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
    Guid ID { get; set; }
    string Name { get; set; }
    DateTime? PurchaseDate { get; set; }
    decimal? PurchasePrice { get; set; }
    DateTime? SellDate { get; set; }
    decimal? SellPrice { get; set; }
    bool Hide { get; set; }
}

public class AssetUpdateRequest() : IAssetUpdateRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? PurchaseDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateTime? SellDate { get; set; } = null;
    public decimal? SellPrice { get; set; } = null;
    public bool Hide { get; set; } = false;
}

public interface IAssetIndexRequest
{
    Guid ID { get; set; }
    int Index { get; set; }
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
