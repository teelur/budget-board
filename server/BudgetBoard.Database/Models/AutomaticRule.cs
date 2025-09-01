namespace BudgetBoard.Database.Models;

public class AutomaticRule()
{
    public Guid ID { get; set; }
    public ICollection<RuleCondition> Conditions { get; set; } = [];
    public ICollection<RuleAction> Actions { get; set; } = [];
    public ApplicationUser? User { get; set; } = null!;
    public Guid UserID { get; set; }
}
