using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class ApplicationUserService(
    ILogger<IApplicationUserService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IApplicationUserService
{
    private readonly ILogger<IApplicationUserService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task<IApplicationUserResponse> ReadApplicationUserAsync(
        Guid userGuid,
        UserManager<ApplicationUser> userManager
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var logins = await userManager.GetLoginsAsync(userData);
        var hasOidcLogin = logins.Any(l => l.LoginProvider == "oidc");
        var hasLocalLogin = logins.Any(l => l.LoginProvider == "local");

        return new ApplicationUserResponse(userData, hasOidcLogin, hasLocalLogin);
    }

    /// <inheritdoc />
    public async Task UpdateApplicationUserAsync(
        Guid userGuid,
        IApplicationUserUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        _userDataContext.Entry(userData).CurrentValues.SetValues(request);
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext.ApplicationUsers.FirstOrDefaultAsync(u =>
                u.Id == new Guid(id)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["UserRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(_responseLocalizer["UserRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
