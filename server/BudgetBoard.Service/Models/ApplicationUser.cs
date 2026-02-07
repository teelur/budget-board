using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IApplicationUserUpdateRequest
{
    DateTime LastSync { get; }
}

public class ApplicationUserUpdateRequest : IApplicationUserUpdateRequest
{
    public DateTime LastSync { get; set; }

    [JsonConstructor]
    public ApplicationUserUpdateRequest()
    {
        LastSync = DateTime.MinValue;
    }
}

public interface IApplicationUserResponse
{
    Guid ID { get; }
    bool SimpleFinAccessToken { get; }
    bool LunchFlowApiKey { get; }
    DateTime LastSync { get; }
    bool TwoFactorEnabled { get; }
    bool HasOidcLogin { get; }
    bool HasLocalLogin { get; }
}

public class ApplicationUserResponse : IApplicationUserResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public bool SimpleFinAccessToken { get; set; } = false;
    public bool LunchFlowApiKey { get; set; } = false;
    public DateTime LastSync { get; set; } = DateTime.MinValue;
    public bool TwoFactorEnabled { get; set; } = false;
    public bool HasOidcLogin { get; set; } = false;
    public bool HasLocalLogin { get; set; } = false;

    [JsonConstructor]
    public ApplicationUserResponse() { }

    public ApplicationUserResponse(
        ApplicationUser user,
        bool hasOidcLogin = false,
        bool hasLocalLogin = false
    )
    {
        ID = user.Id;
        SimpleFinAccessToken = user.SimpleFinAccessToken != string.Empty;
        LunchFlowApiKey = user.LunchFlowApiKey != string.Empty;
        LastSync = user.LastSync;
        TwoFactorEnabled = user.TwoFactorEnabled;
        HasOidcLogin = hasOidcLogin;
        HasLocalLogin = hasLocalLogin;
    }
}
