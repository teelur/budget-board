namespace BudgetBoard.Database.Models.WidgetConfigurations;

/// <summary>
/// Configuration options for the net worth widget.
/// </summary>
public class NetWorthWidgetConfiguration
{
    /// <summary>
    /// Collection of lines displayed in the net worth widget.
    /// </summary>
    public IEnumerable<NetWorthWidgetLine> NetWorthWidgetLines { get; set; } = [];
}

/// <summary>
/// Configuration options for a line within the net worth widget.
/// </summary>
public class NetWorthWidgetLine
{
    /// <summary>
    /// Name of the net worth line.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of category names associated with the line.
    /// </summary>
    public List<string> Categories { get; set; } = [];

    /// <summary>
    /// Group number of the line.
    /// </summary>
    public int Group = 0;

    /// <summary>
    /// Index of the line within its group.
    /// </summary>
    public int Index = 0;
}
