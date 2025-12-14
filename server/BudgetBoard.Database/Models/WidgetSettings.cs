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
    /// Indicates whether the widget is visible on the dashboard.
    /// </summary>
    public bool IsVisible { get; set; } = true;

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
