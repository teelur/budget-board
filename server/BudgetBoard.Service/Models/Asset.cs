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
    DateTime? PurchasedDate { get; set; }
    decimal? PurchasePrice { get; set; }
    DateTime? SoldDate { get; set; }
    decimal? SoldPrice { get; set; }
    bool HideProperty { get; set; }
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
    public DateTime? PurchasedDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateTime? SoldDate { get; set; } = null;
    public decimal? SoldPrice { get; set; } = null;
    public bool HideProperty { get; set; } = false;
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
        PurchasedDate = asset.PurchasedDate;
        PurchasePrice = asset.PurchasePrice;
        SoldDate = asset.SoldDate;
        SoldPrice = asset.SoldPrice;
        HideProperty = asset.HideProperty;
        Deleted = asset.Deleted;
        Index = asset.Index;
        UserID = asset.UserID;
    }
}

public interface IAssetUpdateRequest
{
    Guid ID { get; set; }
    string Name { get; set; }
    DateTime? PurchasedDate { get; set; }
    decimal? PurchasePrice { get; set; }
    DateTime? SoldDate { get; set; }
    decimal? SoldPrice { get; set; }
    bool HideProperty { get; set; }
}

public class AssetUpdateRequest() : IAssetUpdateRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? PurchasedDate { get; set; } = null;
    public decimal? PurchasePrice { get; set; } = null;
    public DateTime? SoldDate { get; set; } = null;
    public decimal? SoldPrice { get; set; } = null;
    public bool HideProperty { get; set; } = false;
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
