using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBoard.Service.Models;

public interface INetWorthWidgetLineCreateRequest
{
    string Name { get; }
    int Group { get; }
    int Index { get; }
    Guid WidgetSettingsId { get; }
}

public class NetWorthWidgetLineCreateRequest : INetWorthWidgetLineCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public int Group { get; set; } = 0;
    public int Index { get; set; } = 0;
    public Guid WidgetSettingsId { get; set; } = Guid.Empty;
}

public interface INetWorthWidgetLineUpdateRequest
{
    Guid LineId { get; }
    string Name { get; }
    int Group { get; }
    int Index { get; }
    Guid WidgetSettingsId { get; }
}

public class NetWorthWidgetLineUpdateRequest : INetWorthWidgetLineUpdateRequest
{
    public Guid LineId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public int Group { get; set; } = 0;
    public int Index { get; set; } = 0;
    public Guid WidgetSettingsId { get; set; } = Guid.Empty;
}
