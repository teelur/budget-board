namespace BudgetBoard.Database.Models;

/// <summary>
/// Supported currency codes.
/// </summary>
[Obsolete("Currency enum is deprecated. Use ISO 4217 currency codes instead.")]
public enum Currency
{
    USD, // US Dollar
    EUR, // Euro
    GBP, // British Pound
    JPY, // Japanese Yen
    CNY, // Chinese Yuan
    INR, // Indian Rupee
    AUD, // Australian Dollar
    CAD, // Canadian Dollar
    CHF, // Swiss Franc
    SEK, // Swedish Krona
    NZD, // New Zealand Dollar
}

public static class SupportedLanguages
{
    public const string SystemDefault = "default";
    public const string EnglishUnitedStates = "en-us";
    public const string German = "de";
    public const string ChineseSimplified = "zh-hans";

    public static List<string> SupportedCultureNames { get; } =
    [EnglishUnitedStates, German, ChineseSimplified];

    public static List<string> AllUserLanguageOptions { get; } =
    [SystemDefault, .. SupportedCultureNames];
}

public class UserSettings
{
    /// <summary>
    /// Unique identifier for the user settings.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The preferred currency code for the user.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the language used for the user.
    /// </summary>
    public string Language { get; set; } = "default";

    /// <summary>
    /// The percentage threshold for budget warnings.
    /// </summary>
    public int BudgetWarningThreshold { get; set; } = 80;

    /// <summary>
    /// Number of months to look back for forced sync operations.
    /// </summary>
    public int ForceSyncLookbackMonths { get; set; } = 0;

    /// <summary>
    /// Indicates whether built-in transaction categories are disabled.
    /// </summary>
    public bool DisableBuiltInTransactionCategories { get; set; } = false;

    /// <summary>
    /// Indicates whether the auto-categorizer is enabled.
    /// </summary>
    public bool EnableAutoCategorizer { get; set; } = false;

    /// <summary>
    /// OID of the auto-categorizer ML model.
    /// </summary>
    public long? AutoCategorizerModelOID { get; set; } = null;

    /// <summary>
    /// Date of most recent auto-categorizer training.
    /// </summary>
    public DateOnly? AutoCategorizerLastTrained { get; set; } = null;

    /// <summary>
    /// Start date of most recent auto-categorizer model.
    /// </summary>
    public DateOnly? AutoCategorizerModelStartDate { get; set; } = null;

    /// <summary>
    /// End date of most recent auto-categorizer model.
    /// </summary>
    public DateOnly? AutoCategorizerModelEndDate { get; set; } = null;

    /// <summary>
    /// Minimum probability percentage for auto-categorizer predictions.
    /// </summary>
    public int AutoCategorizerMinimumProbabilityPercentage { get; set; } = 70;

    /// <summary>
    /// Identifier for the user who owns these settings.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Reference to the owning user.
    /// </summary>
    public ApplicationUser? User { get; set; } = null;
}
