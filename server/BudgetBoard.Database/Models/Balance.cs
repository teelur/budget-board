namespace BudgetBoard.Database.Models;

public class Balance
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime? Deleted { get; set; } = null;
    public required Guid AccountID { get; set; }
    public Account? Account { get; set; } = null;
}
