namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents customizable settings for a dashboard widget.
/// </summary>
public class WidgetSettings
{
    /// <summary>
    /// Unique identifier for the widget settings.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The type of widget (e.g., "NetWorth", "Budget", "Spending").
    /// </summary>
    public required string WidgetType { get; set; }

    /// <summary>
    /// Column position on the grid for large screens (0-based).
    /// </summary>
    public int LgX { get; set; } = 0;

    /// <summary>
    /// Row position on the grid for large screens (0-based).
    /// </summary>
    public int LgY { get; set; } = 0;

    /// <summary>
    /// Width in grid columns for large screens.
    /// </summary>
    public int LgW { get; set; } = 4;

    /// <summary>
    /// Height in grid row units for large screens.
    /// </summary>
    public int LgH { get; set; } = 5;

    /// <summary>
    /// Row position on the grid for small screens (0-based).
    /// </summary>
    public int SmY { get; set; } = 0;

    /// <summary>
    /// Height in grid row units for small screens.
    /// </summary>
    public int SmH { get; set; } = 5;

    /// <summary>
    /// JSON string containing widget-specific configuration options.
    /// </summary>
    public string? Configuration { get; set; } = null;

    /// <summary>
    /// Identifier for the user who owns these widget settings.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
