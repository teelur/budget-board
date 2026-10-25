using System.Text.Json.Serialization;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Models.Widgets.TransactionTrendsWidget;

public class TransactionTrendsWidgetConfiguration
{
    [JsonPropertyName("categoryValues")]
    public List<string> CategoryValues { get; set; } = [];

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; set; } = TransactionCategoryTypes.Expense;
}
