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
    DateTime LastSync { get; }
    bool TwoFactorEnabled { get; }
    bool HasOidcLogin { get; }
    bool HasLocalLogin { get; }
}

public class ApplicationUserResponse : IApplicationUserResponse
{
    public Guid ID { get; set; }
    public bool SimpleFinAccessToken { get; set; }
    public DateTime LastSync { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool HasOidcLogin { get; set; }
    public bool HasLocalLogin { get; set; }

    [JsonConstructor]
    public ApplicationUserResponse()
    {
        ID = new Guid();
        SimpleFinAccessToken = false;
        LastSync = DateTime.MinValue;
        TwoFactorEnabled = false;
        HasOidcLogin = false;
        HasLocalLogin = false;
    }

    public ApplicationUserResponse(
        ApplicationUser user,
        bool hasOidcLogin = false,
        bool hasLocalLogin = false
    )
    {
        ID = user.Id;
        SimpleFinAccessToken = user.SimpleFinAccessToken != string.Empty;
        LastSync = user.LastSync;
        TwoFactorEnabled = user.TwoFactorEnabled;
        HasOidcLogin = hasOidcLogin;
        HasLocalLogin = hasLocalLogin;
    }
}
