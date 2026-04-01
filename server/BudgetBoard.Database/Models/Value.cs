namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents a value record for a financial asset at a specific point in time.
/// </summary>
public class Value
{
    /// <summary>
    /// Unique identifier for the value record.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The value amount.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// The date and time the value was recorded.
    /// </summary>
    public required DateTime DateTime { get; set; }

    /// <summary>
    /// The date and time when the value record was deleted; null if active.
    /// </summary>
    public DateTime? Deleted { get; set; } = null;

    /// <summary>
    /// Identifier for the associated asset.
    /// </summary>
    public required Guid AssetID { get; set; }

    /// <summary>
    /// Reference to the associated asset.
    /// </summary>
    public Asset? Asset { get; set; } = null;
}
