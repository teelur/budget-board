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
    int X { get; }
    int Y { get; }
    int W { get; }
    int H { get; }
}

public class WidgetSettingsCreateRequest : IWidgetSettingsCreateRequest
{
    public string WidgetType { get; set; } = string.Empty;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int W { get; set; } = 4;
    public int H { get; set; } = 5;
}

public class NetWorthWidgetConfiguration
{
    public IEnumerable<NetWorthWidgetGroup> Groups { get; set; } = [];
}

public class NetWorthWidgetGroup
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int Index { get; set; } = 0;
    public IEnumerable<NetWorthWidgetLine> Lines { get; set; } = [];
}

public class NetWorthWidgetLine
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<NetWorthWidgetCategory> Categories { get; set; } = [];
    public int Index { get; set; } = 0;
}

public class NetWorthWidgetCategory
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
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

public interface IWidgetSettingsUpdateRequest<TConfiguration>
    where TConfiguration : class
{
    Guid ID { get; }
    int X { get; }
    int Y { get; }
    int W { get; }
    int H { get; }
    TConfiguration? Configuration { get; }
}

public class WidgetSettingsUpdateRequest<TConfiguration>
    : IWidgetSettingsUpdateRequest<TConfiguration>
    where TConfiguration : class
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int W { get; set; } = 4;
    public int H { get; set; } = 5;
    public TConfiguration? Configuration { get; set; } = null;
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
