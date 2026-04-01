using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IBalanceCreateRequest
{
    decimal Amount { get; }
    DateTime DateTime { get; }
    Guid AccountID { get; }
}

[method: JsonConstructor]
public class BalanceCreateRequest() : IBalanceCreateRequest
{
    public decimal Amount { get; set; } = 0;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    public Guid AccountID { get; set; } = Guid.NewGuid();
}

public interface IBalanceUpdateRequest
{
    Guid ID { get; }
    decimal Amount { get; }
    DateTime DateTime { get; }
    Guid AccountID { get; }
}

[method: JsonConstructor]
public class BalanceUpdateRequest() : IBalanceUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; } = 0;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    public Guid AccountID { get; set; } = Guid.NewGuid();
}

public interface IBalanceResponse
{
    Guid ID { get; }
    decimal Amount { get; }
    DateTime DateTime { get; }
    DateTime? Deleted { get; }
    Guid AccountID { get; }
}

public class BalanceResponse : IBalanceResponse
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime? Deleted { get; set; }
    public Guid AccountID { get; set; }

    [JsonConstructor]
    public BalanceResponse()
    {
        ID = Guid.NewGuid();
        Amount = 0;
        DateTime = DateTime.MinValue;
        Deleted = null;
        AccountID = Guid.NewGuid();
    }

    public BalanceResponse(Balance balance)
    {
        ID = balance.ID;
        Amount = balance.Amount;
        DateTime = balance.DateTime;
        Deleted = balance.Deleted;
        AccountID = balance.AccountID;
    }
}
