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
    /// Column position on the grid (0-based).
    /// </summary>
    public int X { get; set; } = 0;

    /// <summary>
    /// Row position on the grid (0-based).
    /// </summary>
    public int Y { get; set; } = 0;

    /// <summary>
    /// Width in grid columns.
    /// </summary>
    public int W { get; set; } = 4;

    /// <summary>
    /// Height in grid row units.
    /// </summary>
    public int H { get; set; } = 5;

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
