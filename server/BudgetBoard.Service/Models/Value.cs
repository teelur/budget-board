using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IValueCreateRequest
{
    decimal Amount { get; }
    DateOnly Date { get; }
    Guid AssetID { get; }
}

public class ValueCreateRequest() : IValueCreateRequest
{
    public decimal Amount { get; set; } = 0;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid AssetID { get; set; } = Guid.Empty;
}

public interface IValueResponse
{
    Guid ID { get; }
    decimal Amount { get; }
    DateOnly Date { get; }
    Guid AssetID { get; }
}

public class ValueResponse() : IValueResponse
{
    public Guid ID { get; set; } = Guid.Empty;
    public decimal Amount { get; set; } = 0;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public Guid AssetID { get; set; } = Guid.Empty;

    public ValueResponse(Value value)
        : this()
    {
        ID = value.ID;
        Amount = value.Amount;
        Date = value.Date;
        AssetID = value.AssetID;
    }
}

public interface IValueUpdateRequest
{
    Guid ID { get; }
    decimal Amount { get; }
    DateOnly Date { get; }
}

public class ValueUpdateRequest() : IValueUpdateRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public decimal Amount { get; set; } = 0;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
