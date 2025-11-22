namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a transaction category, which may have a parent category.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The category name.
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// The parent category name.
    /// </summary>
    public required string Parent { get; set; }

    /// <summary>
    /// Identifier for the user who owns the category.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
