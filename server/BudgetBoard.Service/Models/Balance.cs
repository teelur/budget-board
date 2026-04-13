using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IBalanceCreateRequest
{
    decimal Amount { get; }
    DateOnly Date { get; }
    Guid AccountID { get; }
}

[method: JsonConstructor]
public class BalanceCreateRequest() : IBalanceCreateRequest
{
    public decimal Amount { get; set; } = 0;
    public DateOnly Date { get; set; } = DateOnly.MinValue;
    public Guid AccountID { get; set; } = Guid.NewGuid();
}

public interface IBalanceUpdateRequest
{
    Guid ID { get; }
    decimal Amount { get; }
    DateOnly Date { get; }
}

[method: JsonConstructor]
public class BalanceUpdateRequest() : IBalanceUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; } = 0;
    public DateOnly Date { get; set; } = DateOnly.MinValue;
}

public interface IBalanceResponse
{
    Guid ID { get; }
    decimal Amount { get; }
    DateOnly Date { get; }
    DateTime? Deleted { get; }
    Guid AccountID { get; }
}

public class BalanceResponse : IBalanceResponse
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public DateTime? Deleted { get; set; }
    public Guid AccountID { get; set; }

    [JsonConstructor]
    public BalanceResponse()
    {
        ID = Guid.NewGuid();
        Amount = 0;
        Date = DateOnly.MinValue;
        Deleted = null;
        AccountID = Guid.NewGuid();
    }

    public BalanceResponse(Balance balance)
    {
        ID = balance.ID;
        Amount = balance.Amount;
        Date = balance.Date;
        Deleted = balance.Deleted;
        AccountID = balance.AccountID;
    }
}
