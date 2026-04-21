namespace BudgetBoard.Service.Models;

public static class WidgetTypes
{
    public const string Accounts = "Accounts";
    public const string NetWorth = "NetWorth";
    public const string UncategorizedTransactions = "UncategorizedTransactions";
    public const string SpendingTrends = "SpendingTrends";

    public static readonly IEnumerable<string> All =
    [
        Accounts,
        NetWorth,
        UncategorizedTransactions,
        SpendingTrends,
    ];
}

public interface IWidgetSettingsCreateRequest
{
    string WidgetType { get; }
    int? X { get; }
    int? Y { get; }
    int? W { get; }
    int? H { get; }
}

public class WidgetSettingsCreateRequest : IWidgetSettingsCreateRequest
{
    public string WidgetType { get; set; } = string.Empty;
    public int? X { get; set; } = null;
    public int? Y { get; set; } = null;
    public int? W { get; set; } = null;
    public int? H { get; set; } = null;
}

public interface IWidgetResponse
{
    Guid ID { get; }
    string WidgetType { get; }
    int X { get; }
    int Y { get; }
    int W { get; }
    int H { get; }
    string Configuration { get; }
    Guid UserID { get; }
}

public class WidgetResponse : IWidgetResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string WidgetType { get; set; } = string.Empty;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int W { get; set; } = 4;
    public int H { get; set; } = 5;
    public string Configuration { get; set; } = string.Empty;
    public Guid UserID { get; set; } = Guid.Empty;
}

public interface IWidgetSettingsUpdateRequest
{
    Guid ID { get; }
    int X { get; }
    int Y { get; }
    int W { get; }
    int H { get; }
    System.Text.Json.JsonElement? Configuration { get; }
}

public class WidgetSettingsUpdateRequest : IWidgetSettingsUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int W { get; set; } = 4;
    public int H { get; set; } = 5;
    public System.Text.Json.JsonElement? Configuration { get; set; } = null;
}

public interface IWidgetSettingsBatchUpdateRequest
{
    Guid ID { get; }
    int X { get; }
    int Y { get; }
    int W { get; }
    int H { get; }
}

public class WidgetSettingsBatchUpdateRequest : IWidgetSettingsBatchUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int W { get; set; } = 4;
    public int H { get; set; } = 5;
}
