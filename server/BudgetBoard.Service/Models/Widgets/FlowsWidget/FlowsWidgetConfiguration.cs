using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models.Widgets.FlowsWidget;

public class FlowsWidgetConfiguration
{
    [JsonPropertyName("monthCount")]
    public int MonthCount { get; set; } = 1;
}
