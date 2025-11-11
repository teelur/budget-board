namespace BudgetBoard.Database.Models;

public class Value
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime? Deleted { get; set; } = null;
    public required Guid AssetID { get; set; }
    public Asset? Asset { get; set; } = null;
}
