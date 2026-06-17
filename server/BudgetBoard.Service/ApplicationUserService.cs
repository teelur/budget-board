using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.AspNetCore.Identity;
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
    public const string OidcLoginProvider = "oidc";

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

        userData.LastSync = request.LastSync;
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DisconnectOidcLoginAsync(
        Guid userGuid,
        UserManager<ApplicationUser> userManager
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var logins = await userManager.GetLoginsAsync(userData);
        var oidcLogin = logins.FirstOrDefault(l => l.LoginProvider == OidcLoginProvider);
        if (oidcLogin == null)
        {
            logger.LogWarning("{LogMessage}", logLocalizer["NoOidcLoginFoundLog", userGuid]);
            throw new BudgetBoardServiceException(responseLocalizer["NoOidcLoginFoundError"]);
        }

        // We can't allow a user to remove OIDC login if they don't have a local account,
        // since they would not be able to login anymore.
        var hasPassword = await userManager.HasPasswordAsync(userData);
        var remainingLogins = logins.Count(l => l.LoginProvider != OidcLoginProvider);
        if (!hasPassword && remainingLogins == 0)
        {
            logger.LogWarning("{LogMessage}", logLocalizer["RemoveOidcNoPasswordLog", userGuid]);
            throw new BudgetBoardServiceException(responseLocalizer["RemoveOidcNoPasswordError"]);
        }

        var result = await userManager.RemoveLoginAsync(
            userData,
            oidcLogin.LoginProvider,
            oidcLogin.ProviderKey
        );

        if (!result.Succeeded)
        {
            logger.LogError(
                "{LogMessage}",
                logLocalizer[
                    "RemoveOidcFailedLog",
                    userGuid,
                    string.Join(", ", result.Errors.Select(e => e.Description))
                ]
            );
            throw new BudgetBoardServiceException(responseLocalizer["RemoveOidcFailedError"]);
        }

        logger.LogInformation("{LogMessage}", logLocalizer["RemoveOidcSuccessLog", userGuid]);
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users => users
        );
    }
}
