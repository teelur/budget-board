using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public static class WidgetTypes
{
    public const string Accounts = "Accounts";
    public const string NetWorth = "NetWorth";
    public const string UncategorizedTransactions = "UncategorizedTransactions";
    public const string SpendingTrends = "SpendingTrends";
    public const string Metric = "Metric";
}

public interface IWidgetSettingsCreateRequest
{
    string WidgetType { get; }
    int? LgX { get; }
    int? LgY { get; }
    int? LgW { get; }
    int? LgH { get; }
    int? SmY { get; }
    int? SmH { get; }
}

public class WidgetSettingsCreateRequest : IWidgetSettingsCreateRequest
{
    public string WidgetType { get; set; } = string.Empty;
    public int? LgX { get; set; } = null;
    public int? LgY { get; set; } = null;
    public int? LgW { get; set; } = null;
    public int? LgH { get; set; } = null;
    public int? SmY { get; set; } = null;
    public int? SmH { get; set; } = null;
}

public interface IWidgetResponse
{
    Guid ID { get; }
    string WidgetType { get; }
    int LgX { get; }
    int LgY { get; }
    int LgW { get; }
    int LgH { get; }
    int SmY { get; }
    int SmH { get; }
    string Configuration { get; }
    Guid UserID { get; }
}

public class WidgetResponse() : IWidgetResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string WidgetType { get; set; } = string.Empty;
    public int LgX { get; set; } = 0;
    public int LgY { get; set; } = 0;
    public int LgW { get; set; } = 4;
    public int LgH { get; set; } = 5;
    public int SmY { get; set; } = 0;
    public int SmH { get; set; } = 5;
    public string Configuration { get; set; } = string.Empty;
    public Guid UserID { get; set; } = Guid.Empty;
}

public interface IWidgetSettingsUpdateRequest
{
    Guid ID { get; }
    int? LgX { get; }
    int? LgY { get; }
    int? LgW { get; }
    int? LgH { get; }
    int? SmY { get; }
    int? SmH { get; }
    OptionalField<System.Text.Json.JsonElement?> Configuration { get; }
}

public class WidgetSettingsUpdateRequest : IWidgetSettingsUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int? LgX { get; set; } = null;
    public int? LgY { get; set; } = null;
    public int? LgW { get; set; } = null;
    public int? LgH { get; set; } = null;
    public int? SmY { get; set; } = null;
    public int? SmH { get; set; } = null;
    public OptionalField<System.Text.Json.JsonElement?> Configuration { get; set; } =
        new OptionalField<System.Text.Json.JsonElement?>();
}
