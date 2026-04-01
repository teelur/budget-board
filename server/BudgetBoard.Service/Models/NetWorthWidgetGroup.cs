namespace BudgetBoard.Service.Models;

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
