using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface IToshlCategoryMappingItem
{
    string ToshlId { get; }
    string ToshlName { get; }
    string ToshlType { get; }
    string ToshlParentName { get; }
    string BudgetBoardCategory { get; }
    string SuggestedBudgetBoardCategory { get; }
}

public class ToshlCategoryMappingItem : IToshlCategoryMappingItem
{
    public string ToshlId { get; set; }
    public string ToshlName { get; set; }
    public string ToshlType { get; set; }
    public string ToshlParentName { get; set; }
    public string BudgetBoardCategory { get; set; }
    public string SuggestedBudgetBoardCategory { get; set; }

    [JsonConstructor]
    public ToshlCategoryMappingItem()
    {
        ToshlId = string.Empty;
        ToshlName = string.Empty;
        ToshlType = string.Empty;
        ToshlParentName = string.Empty;
        BudgetBoardCategory = string.Empty;
        SuggestedBudgetBoardCategory = string.Empty;
    }
}

public interface IToshlCategoryMappingsResponse
{
    IReadOnlyList<ToshlCategoryMappingItem> Items { get; }
}

public class ToshlCategoryMappingsResponse : IToshlCategoryMappingsResponse
{
    public IReadOnlyList<ToshlCategoryMappingItem> Items { get; set; }

    [JsonConstructor]
    public ToshlCategoryMappingsResponse()
    {
        Items = Array.Empty<ToshlCategoryMappingItem>();
    }
}

public interface IToshlCategoryMappingsUpdateRequest
{
    IReadOnlyList<ToshlCategoryMappingUpdateItem> Items { get; }
}

public class ToshlCategoryMappingsUpdateRequest : IToshlCategoryMappingsUpdateRequest
{
    public IReadOnlyList<ToshlCategoryMappingUpdateItem> Items { get; set; }

    [JsonConstructor]
    public ToshlCategoryMappingsUpdateRequest()
    {
        Items = Array.Empty<ToshlCategoryMappingUpdateItem>();
    }
}

public class ToshlCategoryMappingUpdateItem
{
    public string ToshlId { get; set; }
    public string ToshlName { get; set; }
    public string ToshlType { get; set; }
    public string BudgetBoardCategory { get; set; }

    [JsonConstructor]
    public ToshlCategoryMappingUpdateItem()
    {
        ToshlId = string.Empty;
        ToshlName = string.Empty;
        ToshlType = string.Empty;
        BudgetBoardCategory = string.Empty;
    }
}
