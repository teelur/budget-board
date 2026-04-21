namespace BudgetBoard.Service.Models.Widgets.AccountsWidget;

public class AccountsWidgetConfiguration
{
    [System.Text.Json.Serialization.JsonPropertyName("accountIds")]
    public IEnumerable<Guid> AccountIds { get; set; } = [];
}
