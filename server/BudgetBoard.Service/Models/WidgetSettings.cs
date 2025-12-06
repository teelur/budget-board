namespace BudgetBoard.Service.Models;

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
    public IEnumerable<NetWorthWidgetLine> Lines { get; set; } = [];
}

public class NetWorthWidgetLine
{
    public string Name { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = [];
    public int Group = 0;
    public int Index = 0;
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
    string WidgetType { get; }
    bool IsVisible { get; }
    TConfiguration Configuration { get; }
    Guid UserID { get; }
}

public class WidgetSettingsUpdateRequest<TConfiguration>
    : IWidgetSettingsUpdateRequest<TConfiguration>
    where TConfiguration : class
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string WidgetType { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public TConfiguration Configuration { get; set; } = null!;
    public Guid UserID { get; set; } = Guid.Empty;
}
