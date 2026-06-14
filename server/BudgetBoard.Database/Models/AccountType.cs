namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a type of account.
/// </summary>
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
    /// Used to classify accounts into broader categories (e.g., "Asset", "Liability").
    /// </summary>
    public string Classification { get; set; } = AccountTypeClassification.Asset;

    /// <summary>
    /// Identifier for the user who owns the account type.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
