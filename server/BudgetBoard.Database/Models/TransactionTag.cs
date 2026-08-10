namespace BudgetBoard.Database.Models;

public class TransactionTag
{
    public required Guid TransactionID { get; set; }
    public required Guid TagID { get; set; }

    public Transaction? Transaction { get; set; }
    public Tag? Tag { get; set; }
}
