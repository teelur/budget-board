using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IUserSettingsResponse
{
    string Currency { get; }
    int BudgetWarningThreshold { get; }
    int ForceSyncLookbackMonths { get; }
}

public class UserSettingsResponse : IUserSettingsResponse
{
    public string Currency { get; set; }
    public int BudgetWarningThreshold { get; set; }
    public int ForceSyncLookbackMonths { get; set; }

    [JsonConstructor]
    public UserSettingsResponse()
    {
        Currency = "USD";
        BudgetWarningThreshold = 80;
        ForceSyncLookbackMonths = 0;
    }

    public UserSettingsResponse(UserSettings userSettings)
    {
        Currency = userSettings.Currency.ToString();
        BudgetWarningThreshold = userSettings.BudgetWarningThreshold;
        ForceSyncLookbackMonths = userSettings.ForceSyncLookbackMonths;
    }
}

public interface IUserSettingsUpdateRequest
{
    public string? Currency { get; }
    public int? BudgetWarningThreshold { get; }
    public int? ForceSyncLookbackMonths { get; }
}

public class UserSettingsUpdateRequest : IUserSettingsUpdateRequest
{
    public string? Currency { get; set; }
    public int? BudgetWarningThreshold { get; set; }
    public int? ForceSyncLookbackMonths { get; set; }

    [JsonConstructor]
    public UserSettingsUpdateRequest()
    {
        Currency = null;
        BudgetWarningThreshold = null;
        ForceSyncLookbackMonths = null;
    }
}
