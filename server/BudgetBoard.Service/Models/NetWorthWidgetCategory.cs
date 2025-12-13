using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBoard.Service.Models;

public interface INetWorthWidgetCategoryCreateRequest
{
    string Value { get; }
    string Type { get; }
    string Subtype { get; }
    public Guid LineId { get; }
    public Guid WidgetSettingsId { get; }
}

public class NetWorthWidgetCategoryCreateRequest : INetWorthWidgetCategoryCreateRequest
{
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public Guid LineId { get; set; } = Guid.Empty;
    public Guid WidgetSettingsId { get; set; } = Guid.Empty;
}

public interface INetWorthWidgetCategoryUpdateRequest
{
    Guid Id { get; }
    string Value { get; }
    string Type { get; }
    string Subtype { get; }
    Guid LineId { get; }
    Guid WidgetSettingsId { get; }
}

public class NetWorthWidgetCategoryUpdateRequest : INetWorthWidgetCategoryUpdateRequest
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public Guid LineId { get; set; } = Guid.Empty;
    public Guid WidgetSettingsId { get; set; } = Guid.Empty;
}
