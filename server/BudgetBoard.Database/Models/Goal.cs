namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a financial goal set by the user.
/// </summary>
public class Goal
{
    /// <summary>
    /// Unique identifier for the goal.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name or description of the goal.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The target completion date for the goal, if any.
    /// </summary>
    public DateTime? CompleteDate { get; set; } = null;

    /// <summary>
    /// The target amount for the goal.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// The initial amount when the goal was created.
    /// </summary>
    public required decimal InitialAmount { get; set; }

    /// <summary>
    /// The monthly contribution towards the goal, if any.
    /// </summary>
    public decimal? MonthlyContribution { get; set; } = null;

    /// <summary>
    /// The date the goal was completed, if any.
    /// </summary>
    public DateTime? Completed { get; set; } = null;

    /// <summary>
    /// Accounts associated with the goal.
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = [];

    /// <summary>
    /// Identifier for the user who owns the goal.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
