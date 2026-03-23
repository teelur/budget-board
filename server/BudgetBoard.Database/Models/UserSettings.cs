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
    public const string French = "fr";
    public const string ChineseSimplified = "zh-hans";

    public static List<string> SupportedCultureNames { get; } =
    [EnglishUnitedStates, German, French, ChineseSimplified];

    public static List<string> AllUserLanguageOptions { get; } =
    [SystemDefault, .. SupportedCultureNames];
}

public static class ToshlMetadataSyncDirections
{
    public const string BudgetBoard = "budgetboard";
    public const string Toshl = "toshl";

    public static List<string> AllOptions { get; } = [BudgetBoard, Toshl];
}

public static class ToshlFullSyncStatuses
{
    public const string Idle = "idle";
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";

    public static List<string> AllOptions { get; } = [Idle, Queued, Running, Succeeded, Failed];
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
    /// Gets or sets the date format used for the user.
    /// </summary>
    public string DateFormat { get; set; } = "default";

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
    /// Controls which metadata source is authoritative for Toshl-linked data.
    /// </summary>
    public string ToshlMetadataSyncDirection { get; set; } =
        ToshlMetadataSyncDirections.Toshl;

    /// <summary>
    /// Controls how far back Toshl transaction imports should look.
    /// A value of 0 imports all available history.
    /// </summary>
    public int ToshlSyncLookbackMonths { get; set; } = 0;

    /// <summary>
    /// Controls how often Toshl metadata is synchronized automatically.
    /// </summary>
    public int ToshlAutoSyncIntervalHours { get; set; } = 8;

    /// <summary>
    /// Serialized Toshl category and tag mappings to Budget Board categories.
    /// </summary>
    public string ToshlCategoryMappingsJson { get; set; } = "[]";

    /// <summary>
    /// Status of the latest user-triggered Toshl full sync.
    /// </summary>
    public string ToshlFullSyncStatus { get; set; } = ToshlFullSyncStatuses.Idle;

    /// <summary>
    /// Time when the latest user-triggered Toshl full sync was queued.
    /// </summary>
    public DateTime? ToshlFullSyncQueuedAt { get; set; } = null;

    /// <summary>
    /// Time when the latest user-triggered Toshl full sync started.
    /// </summary>
    public DateTime? ToshlFullSyncStartedAt { get; set; } = null;

    /// <summary>
    /// Time when the latest user-triggered Toshl full sync completed.
    /// </summary>
    public DateTime? ToshlFullSyncCompletedAt { get; set; } = null;

    /// <summary>
    /// Detailed failure text for the latest user-triggered Toshl full sync.
    /// </summary>
    public string ToshlFullSyncError { get; set; } = string.Empty;

    /// <summary>
    /// Coarse completion percentage for the latest user-triggered Toshl full sync.
    /// </summary>
    public int ToshlFullSyncProgressPercent { get; set; } = 0;

    /// <summary>
    /// Human-readable progress text for the latest user-triggered Toshl full sync.
    /// </summary>
    public string ToshlFullSyncProgressDescription { get; set; } = string.Empty;

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
