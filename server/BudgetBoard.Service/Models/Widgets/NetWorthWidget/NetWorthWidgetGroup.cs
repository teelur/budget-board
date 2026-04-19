namespace BudgetBoard.Service.Models.Widgets.NetWorthWidget;

public class NetWorthWidgetGroup
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int Index { get; set; } = 0;
    public IEnumerable<NetWorthWidgetLine> Lines { get; set; } = [];
}

public interface INetWorthWidgetGroupReorderRequest
{
    Guid WidgetSettingsId { get; }
    IEnumerable<Guid> OrderedGroupIds { get; }
}

public class NetWorthWidgetGroupReorderRequest : INetWorthWidgetGroupReorderRequest
{
    public Guid WidgetSettingsId { get; set; } = Guid.Empty;
    public IEnumerable<Guid> OrderedGroupIds { get; set; } = [];
}
