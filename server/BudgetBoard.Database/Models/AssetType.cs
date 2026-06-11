namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a type of asset.
/// </summary>
public class AssetType
{
    /// <summary>
    /// Unique identifier for the asset type.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The asset type name.
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// The parent asset type name.
    /// </summary>
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Identifier for the user who owns the asset type.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
