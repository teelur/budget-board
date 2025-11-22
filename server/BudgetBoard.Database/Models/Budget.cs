namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a budget for a specific category and date.
/// </summary>
public class Budget
{
    /// <summary>
    /// Unique identifier for the budget.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The date the budget applies to.
    /// </summary>
    public required DateTime Date { get; set; }

    /// <summary>
    /// The category for which the budget is set.
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// The spending limit for the category.
    /// </summary>
    public required decimal Limit { get; set; }

    /// <summary>
    /// Identifier for the user who owns the budget.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
