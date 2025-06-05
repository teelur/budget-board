using BudgetBoard.Database.Models;
using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface IApplicationUserUpdateRequest
{
    DateTime LastSync { get; set; }
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
    Guid ID { get; set; }
    bool AccessToken { get; set; }
    DateTime LastSync { get; set; }
    bool TwoFactorEnabled { get; set; }
}

public class ApplicationUserResponse : IApplicationUserResponse
{
    public Guid ID { get; set; }
    public bool AccessToken { get; set; }
    public DateTime LastSync { get; set; }
    public bool TwoFactorEnabled { get; set; }

    [JsonConstructor]
    public ApplicationUserResponse()
    {
        ID = new Guid();
        AccessToken = false;
        LastSync = DateTime.MinValue;
        TwoFactorEnabled = false;
    }

    public ApplicationUserResponse(ApplicationUser user)
    {
        ID = user.Id;
        AccessToken = (user.AccessToken != string.Empty);
        LastSync = user.LastSync;
        TwoFactorEnabled = user.TwoFactorEnabled;
    }
}
