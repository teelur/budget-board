namespace BudgetBoard.Database.Models;

public class Value
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public required Guid PropertyID { get; set; }
    public Property? Property { get; set; } = null;
}