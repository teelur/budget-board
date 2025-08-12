namespace BudgetBoard.Database.Models;

public class AutomaticCategorizationRule()
{
    public Guid ID { get; set; }
    public string CategorizationRule { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; } = null!;
    public Guid UserID { get; set; }
}
