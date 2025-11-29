namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents an automatic rule for transaction processing, containing conditions and actions.
/// </summary>
public class AutomaticRule
{
    /// <summary>
    /// Unique identifier for the rule.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Collection of conditions that must be met for the rule to trigger.
    /// </summary>
    public ICollection<RuleCondition> Conditions { get; set; } = [];

    /// <summary>
    /// Collection of actions to perform when the rule is triggered.
    /// </summary>
    public ICollection<RuleAction> Actions { get; set; } = [];

    /// <summary>
    /// Reference to the user who owns the rule.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;

    /// <summary>
    /// Identifier for the user who owns the rule.
    /// </summary>
    public required Guid UserID { get; set; }
}
