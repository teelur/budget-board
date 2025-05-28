using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IUserSettingsResponse
{
    string Currency { get; set; }
}

public class UserSettingsResponse : IUserSettingsResponse
{
    public string Currency { get; set; }

    [JsonConstructor]
    public UserSettingsResponse()
    {
        Currency = Database.Models.Currency.USD.ToString();
    }

    public UserSettingsResponse(UserSettings userSettings)
    {
        Currency = userSettings.Currency.ToString();
    }
}

public interface IUserSettingsUpdateRequest
{
    public string Currency { get; set; }
}

public class UserSettingsUpdateRequest : IUserSettingsUpdateRequest
{
    public string Currency { get; set; }

    [JsonConstructor]
    public UserSettingsUpdateRequest()
    {
        Currency = Database.Models.Currency.USD.ToString();
    }
}
