using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IAssetType
{
    public string Value { get; }
    public string Parent { get; }
}

public class AssetTypeBase : IAssetType
{
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AssetTypeBase()
    {
        Value = string.Empty;
        Parent = string.Empty;
    }
}

public interface IAssetTypeCreateRequest : IAssetType { }

public class AssetTypeCreateRequest : IAssetTypeCreateRequest
{
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AssetTypeCreateRequest()
    {
        Value = string.Empty;
        Parent = string.Empty;
    }
}

public interface IAssetTypeUpdateRequest
{
    Guid ID { get; }
    string Value { get; }
    string Parent { get; }
}

public class AssetTypeUpdateRequest : IAssetTypeUpdateRequest
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AssetTypeUpdateRequest()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
    }
}

public interface IAssetTypeResponse
{
    Guid ID { get; }
    string Value { get; }
    string Parent { get; }
}

public class AssetTypeResponse : IAssetTypeResponse
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AssetTypeResponse()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
    }

    public AssetTypeResponse(AssetType assetType)
    {
        ID = assetType.ID;
        Value = assetType.Value;
        Parent = assetType.Parent;
    }

    public AssetTypeResponse(IAssetType assetType)
    {
        ID = Guid.Empty;
        Value = assetType.Value;
        Parent = assetType.Parent;
    }
}
