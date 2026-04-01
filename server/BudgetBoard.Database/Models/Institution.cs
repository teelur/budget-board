namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a financial institution associated with the user.
/// </summary>
public class Institution
{
    /// <summary>
    /// Unique identifier for the institution.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the institution.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional ordering index for the institution.
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// The date and time when the institution was deleted; null if active.
    /// </summary>
    public DateTime? Deleted { get; set; } = null;

    /// <summary>
    /// Identifier for the user who owns the institution.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;

    /// <summary>
    /// Accounts associated with the institution.
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = [];
}
