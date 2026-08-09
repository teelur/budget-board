namespace BudgetBoard.Database.Models;

public class Tag
{
    public const int MaxValueLength = 50;

    public Guid ID { get; set; } = Guid.NewGuid();
    public required Guid UserID { get; set; }
    public required string Value { get; set; }
    public required string NormalizedValue { get; set; }

    public ApplicationUser? User { get; set; }
    public ICollection<TransactionTag> TransactionTags { get; set; } = [];
}
