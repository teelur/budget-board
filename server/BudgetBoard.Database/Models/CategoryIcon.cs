namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents the emoji a user has assigned to a transaction category.
/// </summary>
public class CategoryIcon
{
    /// <summary>
    /// The maximum length of an icon value.
    /// </summary>
    public const int MaxIconLength = 32;

    /// <summary>
    /// Unique identifier for the category icon.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The name of the category the icon belongs to.
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// The emoji displayed for the category.
    /// </summary>
    public required string Icon { get; set; }

    /// <summary>
    /// Identifier for the user who owns the category icon.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
