namespace BudgetBoard.Service.Models;

public static class WidgetTypes
{
    public const string NetWorth = "NetWorth";

    public static readonly IEnumerable<string> All = [NetWorth];
}

public interface IWidgetSettingsCreateRequest<TConfiguration>
    where TConfiguration : class
{
    string WidgetType { get; }
    bool IsVisible { get; }
    TConfiguration Configuration { get; }
    Guid UserID { get; }
}

public class WidgetSettingsCreateRequest<TConfiguration>
    : IWidgetSettingsCreateRequest<TConfiguration>
    where TConfiguration : class
{
    public string WidgetType { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public TConfiguration Configuration { get; set; } = null!;
    public Guid UserID { get; set; } = Guid.Empty;
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
    bool IsVisible { get; }
    string Configuration { get; }
    Guid UserID { get; }
}

public class WidgetResponse : IWidgetResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string WidgetType { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public string Configuration { get; set; } = string.Empty;
    public Guid UserID { get; set; } = Guid.Empty;
}

public interface IWidgetSettingsUpdateRequest<TConfiguration>
    where TConfiguration : class
{
    Guid ID { get; }
    bool IsVisible { get; }
    TConfiguration Configuration { get; }
}

public class WidgetSettingsUpdateRequest<TConfiguration>
    : IWidgetSettingsUpdateRequest<TConfiguration>
    where TConfiguration : class
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public bool IsVisible { get; set; } = true;
    public TConfiguration Configuration { get; set; } = null!;
}
