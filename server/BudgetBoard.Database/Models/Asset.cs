namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a financial asset within the budgeting application.
/// </summary>
public class Asset
{
    /// <summary>
    /// Unique identifier for the asset.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name or description of the asset.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Date the asset was purchased, or null if unknown.
    /// </summary>
    public DateTime? PurchaseDate { get; set; } = null;

    /// <summary>
    /// Price paid for the asset at purchase, or null if unknown.
    /// </summary>
    public decimal? PurchasePrice { get; set; } = null;

    /// <summary>
    /// Date the asset was sold, or null if not sold.
    /// </summary>
    public DateTime? SellDate { get; set; } = null;

    /// <summary>
    /// Price received when the asset was sold, or null if not sold.
    /// </summary>
    public decimal? SellPrice { get; set; } = null;

    /// <summary>
    /// Indicates whether the asset is hidden from views.
    /// </summary>
    public bool Hide { get; set; } = false;

    /// <summary>
    /// The date and time when the asset was deleted; null if active.
    /// </summary>
    public DateTime? Deleted { get; set; } = null;

    /// <summary>
    /// Optional ordering index for the asset.
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// Historical or current values associated with the asset.
    /// </summary>
    public ICollection<Value> Values { get; set; } = [];

    /// <summary>
    /// Identifier for the user who owns the asset.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
