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

public class ApplicationUserResponse(
    ApplicationUser user,
    bool hasOidcLogin = false,
    bool hasLocalLogin = false
) : IApplicationUserResponse
{
    public Guid ID { get; set; } = user.Id;
    public bool SimpleFinAccessToken { get; set; } = user.SimpleFinAccessToken != string.Empty;
    public bool LunchFlowApiKey { get; set; } = user.LunchFlowApiKey != string.Empty;
    public DateTime LastSync { get; set; } = user.LastSync;
    public bool TwoFactorEnabled { get; set; } = user.TwoFactorEnabled;
    public bool HasOidcLogin { get; set; } = hasOidcLogin;
    public bool HasLocalLogin { get; set; } = hasLocalLogin;
}
