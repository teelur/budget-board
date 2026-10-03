using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IBudgetCreateRequest
{
    DateOnly Month { get; }
    string Category { get; }
    decimal Limit { get; }
    DateOnly? RolloverStartMonth { get; }
}

public class BudgetCreateRequest : IBudgetCreateRequest
{
    public DateOnly Month { get; set; } = DateOnly.MinValue;
    public string Category { get; set; } = string.Empty;
    public decimal Limit { get; set; } = 0;
    public DateOnly? RolloverStartMonth { get; set; } = null;
}

public interface IBudgetUpdateRequest
{
    Guid ID { get; }
    decimal Limit { get; }
    DateOnly? RolloverStartMonth { get; }
}

public class BudgetUpdateRequest : IBudgetUpdateRequest
{
    public Guid ID { get; set; }
    public decimal Limit { get; set; }
    public DateOnly? RolloverStartMonth { get; set; }

    [JsonConstructor]
    public BudgetUpdateRequest()
    {
        ID = Guid.NewGuid();
        Limit = 0;
        RolloverStartMonth = null;
    }
}

public interface IBudgetResponse
{
    Guid ID { get; }
    DateOnly Month { get; }
    string Category { get; }
    decimal Limit { get; }
    DateOnly? RolloverStartMonth { get; }
    decimal Rollover { get; }
    Guid UserID { get; }
}

public class BudgetResponse : IBudgetResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public DateOnly Month { get; set; } = DateOnly.MinValue;
    public string Category { get; set; } = string.Empty;
    public decimal Limit { get; set; } = 0;
    public DateOnly? RolloverStartMonth { get; set; } = null;

    /// <summary>
    /// The balance accumulated from the rollover start month up to this budget's month.
    /// Negative when earlier months were overspent.
    /// </summary>
    public decimal Rollover { get; set; } = 0;
    public Guid UserID { get; set; } = Guid.NewGuid();

    public BudgetResponse(Budget budget, decimal rollover = 0)
    {
        ID = budget.ID;
        Month = budget.Month;
        Category = budget.Category;
        Limit = budget.Limit;
        RolloverStartMonth = budget.RolloverStartMonth;
        Rollover = rollover;
        UserID = budget.UserID;
    }
}
