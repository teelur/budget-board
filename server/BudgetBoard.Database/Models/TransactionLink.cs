namespace BudgetBoard.Database.Models;

public class TransactionLink
{
    public Guid ID { get; set; } = Guid.NewGuid();

    public required Guid SourceTransactionID { get; set; }
    public Transaction? SourceTransaction { get; set; }

    public required Guid TargetTransactionID { get; set; }
    public Transaction? TargetTransaction { get; set; }
}
