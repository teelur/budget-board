using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IUserSettingsResponse
{
    string Currency { get; }
    int BudgetWarningThreshold { get; }
    int ForceSyncLookbackMonths { get; }
    bool DisableBuiltInTransactionCategories { get; }
}

public class UserSettingsResponse : IUserSettingsResponse
{
    public string Currency { get; set; }
    public int BudgetWarningThreshold { get; set; }
    public int ForceSyncLookbackMonths { get; set; }
    public bool DisableBuiltInTransactionCategories { get; set; }

    [JsonConstructor]
    public UserSettingsResponse()
    {
        Currency = "USD";
        BudgetWarningThreshold = 80;
        ForceSyncLookbackMonths = 0;
        DisableBuiltInTransactionCategories = false;
    }

    public UserSettingsResponse(UserSettings userSettings)
    {
        Currency = userSettings.Currency.ToString();
        BudgetWarningThreshold = userSettings.BudgetWarningThreshold;
        ForceSyncLookbackMonths = userSettings.ForceSyncLookbackMonths;
        DisableBuiltInTransactionCategories = userSettings.DisableBuiltInTransactionCategories;
    }
}

public interface IUserSettingsUpdateRequest
{
    public string? Currency { get; }
    public int? BudgetWarningThreshold { get; }
    public int? ForceSyncLookbackMonths { get; }
    public bool? DisableBuiltInTransactionCategories { get; }
}

[method: JsonConstructor]
public class UserSettingsUpdateRequest() : IUserSettingsUpdateRequest
{
    public string? Currency { get; set; } = null;
    public int? BudgetWarningThreshold { get; set; } = null;
    public int? ForceSyncLookbackMonths { get; set; } = null;
    public bool? DisableBuiltInTransactionCategories { get; set; } = null;
}
