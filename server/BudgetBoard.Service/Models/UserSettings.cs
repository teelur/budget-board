using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IUserSettingsResponse
{
    string Currency { get; }
    int BudgetWarningThreshold { get; }
}

public class UserSettingsResponse : IUserSettingsResponse
{
    public string Currency { get; set; }
    public int BudgetWarningThreshold { get; set; }

    [JsonConstructor]
    public UserSettingsResponse()
    {
        Currency = "USD";
        BudgetWarningThreshold = 80;
    }

    public UserSettingsResponse(UserSettings userSettings)
    {
        Currency = userSettings.Currency.ToString();
        BudgetWarningThreshold = userSettings.BudgetWarningThreshold;
    }
}

public interface IUserSettingsUpdateRequest
{
    public string? Currency { get; }
    public int? BudgetWarningThreshold { get; }
}

public class UserSettingsUpdateRequest : IUserSettingsUpdateRequest
{
    public string? Currency { get; set; }
    public int? BudgetWarningThreshold { get; set; }

    [JsonConstructor]
    public UserSettingsUpdateRequest()
    {
        Currency = "USD";
        BudgetWarningThreshold = 80;
    }
}
