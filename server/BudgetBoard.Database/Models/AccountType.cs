namespace BudgetBoard.Database.Models;

public class AccountType
{
    /// <summary>
    /// Unique identifier for the account type.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The account type name.
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// The parent account type name.
    /// </summary>
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// The account classification.
    /// </summary>
    public string Classification { get; set; } = string.Empty;

    /// <summary>
    /// Identifier for the user who owns the account type.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
