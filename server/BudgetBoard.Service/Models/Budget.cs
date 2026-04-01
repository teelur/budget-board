using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IBudgetCreateRequest
{
    DateTime Date { get; }
    string Category { get; }
    decimal Limit { get; }
}

public class BudgetCreateRequest : IBudgetCreateRequest
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public decimal Limit { get; set; }

    [JsonConstructor]
    public BudgetCreateRequest()
    {
        Date = DateTime.MinValue;
        Category = string.Empty;
        Limit = 0;
    }
}

public interface IBudgetUpdateRequest
{
    Guid ID { get; }
    decimal Limit { get; }
}

public class BudgetUpdateRequest : IBudgetUpdateRequest
{
    public Guid ID { get; set; }
    public decimal Limit { get; set; }

    [JsonConstructor]
    public BudgetUpdateRequest()
    {
        ID = Guid.NewGuid();
        Limit = 0;
    }
}

public interface IBudgetResponse
{
    Guid ID { get; }
    DateTime Date { get; }
    string Category { get; }
    decimal Limit { get; }
    Guid UserID { get; }
}

public class BudgetResponse : IBudgetResponse
{
    public Guid ID { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public decimal Limit { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public BudgetResponse()
    {
        ID = Guid.NewGuid();
        Date = DateTime.MinValue;
        Category = string.Empty;
        Limit = 0;
        UserID = Guid.NewGuid();
    }

    public BudgetResponse(Budget budget)
    {
        ID = budget.ID;
        Date = budget.Date;
        Category = budget.Category;
        Limit = budget.Limit;
        UserID = budget.UserID;
    }
}
