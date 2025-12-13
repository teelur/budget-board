using Microsoft.AspNetCore.Identity;

namespace BudgetBoard.Database.Models;

/// <summary>
/// Represents an application user within the budgeting application.
/// </summary>
/// <remarks>
/// Inherits from IdentityUser to integrate with ASP.NET Core Identity for authentication and authorization.
/// </remarks>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Access token for external service integrations.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The last date and time the user's data was synchronized.
    /// </summary>
    public DateTime LastSync { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Collection of financial accounts owned by the user.
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = [];

    /// <summary>
    /// Collection of budgets created by the user.
    /// </summary>
    public ICollection<Budget> Budgets { get; set; } = [];

    /// <summary>
    /// Collection of financial goals set by the user.
    /// </summary>
    public ICollection<Goal> Goals { get; set; } = [];

    /// <summary>
    /// Collection of transaction categories defined by the user.
    /// </summary>
    public ICollection<Category> TransactionCategories { get; set; } = [];

    /// <summary>
    /// Collection of financial institutions associated with the user.
    /// </summary>
    public ICollection<Institution> Institutions { get; set; } = [];

    /// <summary>
    /// User-specific settings and preferences.
    /// </summary>
    public UserSettings? UserSettings { get; set; } = null;

    /// <summary>
    /// Collection of automatic rules for transaction processing.
    /// </summary>
    public ICollection<AutomaticRule> AutomaticRules { get; set; } = [];

    /// <summary>
    /// Collection of financial assets owned by the user.
    /// </summary>
    public ICollection<Asset> Assets { get; set; } = [];

    /// <summary>
    /// Collection of widget settings for the user's dashboard.
    /// </summary>
    public ICollection<WidgetSettings> WidgetSettings { get; set; } = [];
}
