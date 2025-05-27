using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface IUserSettingsResponse
{
    char Currency { get; set; }
}

public class UserSettingsResponse : IUserSettingsResponse
{
    public char Currency { get; set; }

    [JsonConstructor]
    public UserSettingsResponse()
    {
        Currency = '$';
    }

    public UserSettingsResponse(Database.Models.UserSettings userSettings)
    {
        Currency = userSettings.Currency;
    }
}

public interface IUserSettingsUpdateRequest
{
    public char Currency { get; set; }
}

public class UserSettingsUpdateRequest : IUserSettingsUpdateRequest
{
    public char Currency { get; set; }

    [JsonConstructor]
    public UserSettingsUpdateRequest()
    {
        Currency = '$';
    }
}
