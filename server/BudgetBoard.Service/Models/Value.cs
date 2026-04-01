using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IValueCreateRequest
{
    decimal Amount { get; }
    DateTime DateTime { get; }
    Guid AssetID { get; }
}

public class ValueCreateRequest() : IValueCreateRequest
{
    public decimal Amount { get; set; } = 0;
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public Guid AssetID { get; set; } = Guid.Empty;
}

public interface IValueResponse
{
    Guid ID { get; }
    decimal Amount { get; }
    DateTime DateTime { get; }
    DateTime? Deleted { get; }
    Guid AssetID { get; }
}

public class ValueResponse() : IValueResponse
{
    public Guid ID { get; set; } = Guid.Empty;
    public decimal Amount { get; set; } = 0;
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public DateTime? Deleted { get; set; } = null;
    public Guid AssetID { get; set; } = Guid.Empty;

    public ValueResponse(Value value)
        : this()
    {
        ID = value.ID;
        Amount = value.Amount;
        DateTime = value.DateTime;
        Deleted = value.Deleted;
        AssetID = value.AssetID;
    }
}

public interface IValueUpdateRequest
{
    Guid ID { get; }
    decimal Amount { get; }
    DateTime DateTime { get; }
}

public class ValueUpdateRequest() : IValueUpdateRequest
{
    public Guid ID { get; set; } = Guid.Empty;
    public decimal Amount { get; set; } = 0;
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}
