namespace BudgetBoard.Database.Models;

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

public class UserSettings()
{
    public Guid ID { get; set; }
    public string Currency { get; set; } = "USD";
    public int BudgetWarningThreshold { get; set; } = 80;
    public int ForceSyncLookbackMonths { get; set; } = 0;
    public bool DisableBuiltInTransactionCategories { get; set; } = false;
    public Guid UserID { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
