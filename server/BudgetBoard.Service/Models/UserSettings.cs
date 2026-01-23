using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IUserSettingsResponse
{
    string Currency { get; }
    string Language { get; }
    int BudgetWarningThreshold { get; }
    int ForceSyncLookbackMonths { get; }
    bool DisableBuiltInTransactionCategories { get; }
}

public class UserSettingsResponse : IUserSettingsResponse
{
    public string Currency { get; set; }
    public string Language { get; set; }
    public int BudgetWarningThreshold { get; set; }
    public int ForceSyncLookbackMonths { get; set; }
    public bool DisableBuiltInTransactionCategories { get; set; }
    public bool EnableAutoCategorizer { get; set; }
    public long? AutoCategorizerModelOID { get; set; }
    public DateOnly? AutoCategorizerLastTrained { get; set; }
    public DateOnly? AutoCategorizerModelStartDate { get; set; }
    public DateOnly? AutoCategorizerModelEndDate { get; set; }

    [JsonConstructor]
    public UserSettingsResponse()
    {
        Currency = "USD";
        Language = "default";
        BudgetWarningThreshold = 80;
        ForceSyncLookbackMonths = 0;
        DisableBuiltInTransactionCategories = false;
        EnableAutoCategorizer = false;
    }

    public UserSettingsResponse(UserSettings userSettings)
    {
        Currency = userSettings.Currency.ToString();
        Language = userSettings.Language.ToString();
        BudgetWarningThreshold = userSettings.BudgetWarningThreshold;
        ForceSyncLookbackMonths = userSettings.ForceSyncLookbackMonths;
        DisableBuiltInTransactionCategories = userSettings.DisableBuiltInTransactionCategories;
        EnableAutoCategorizer = userSettings.EnableAutoCategorizer;
        AutoCategorizerModelOID = userSettings.AutoCategorizerModelOID;
        AutoCategorizerLastTrained = userSettings.AutoCategorizerLastTrained;
        AutoCategorizerModelStartDate = userSettings.AutoCategorizerModelStartDate;
        AutoCategorizerModelEndDate = userSettings.AutoCategorizerModelEndDate;
    }
}

public interface IUserSettingsUpdateRequest
{
    public string? Currency { get; }
    public string? Language { get; }
    public int? BudgetWarningThreshold { get; }
    public int? ForceSyncLookbackMonths { get; }
    public bool? DisableBuiltInTransactionCategories { get; }
    public bool? EnableAutoCategorizer { get; }
    public long? AutoCategorizerModelOID { get; }
    public DateOnly? AutoCategorizerLastTrained { get; }
    public DateOnly? AutoCategorizerModelStartDate { get; }
    public DateOnly? AutoCategorizerModelEndDate { get; }
}

[method: JsonConstructor]
public class UserSettingsUpdateRequest() : IUserSettingsUpdateRequest
{
    public string? Currency { get; set; } = null;
    public string? Language { get; set; } = null;
    public int? BudgetWarningThreshold { get; set; } = null;
    public int? ForceSyncLookbackMonths { get; set; } = null;
    public bool? DisableBuiltInTransactionCategories { get; set; } = null;
    public bool? EnableAutoCategorizer { get; set; } = null;
    public long? AutoCategorizerModelOID { get; set; } = null;
    public DateOnly? AutoCategorizerLastTrained { get; set; } = null;
    public DateOnly? AutoCategorizerModelStartDate { get; set; } = null;
    public DateOnly? AutoCategorizerModelEndDate { get; set; } = null;
}
