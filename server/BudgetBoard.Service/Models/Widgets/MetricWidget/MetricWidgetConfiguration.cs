using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models.Widgets.MetricWidget;

public class MetricWidgetConfiguration
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
