using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class ApplicationUserService(
    ILogger<IApplicationUserService> logger,
    UserDataContext userDataContext
) : IApplicationUserService
{
    private readonly ILogger<IApplicationUserService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    /// <inheritdoc />
    public async Task<IApplicationUserResponse> ReadApplicationUserAsync(
        Guid userGuid,
        UserManager<ApplicationUser> userManager
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        // Check if user has OIDC login
        var logins = await userManager.GetLoginsAsync(userData);
        var hasOidcLogin = logins.Any(l => l.LoginProvider == "oidc");
        var hasLocalLogin = logins.Any(l => l.LoginProvider == "local");

        return new ApplicationUserResponse(userData, hasOidcLogin, hasLocalLogin);
    }

    /// <inheritdoc />
    public async Task UpdateApplicationUserAsync(Guid userGuid, IApplicationUserUpdateRequest user)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        userData.LastSync = user.LastSync;

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext.ApplicationUsers.ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while retrieving the user data: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while retrieving the user data."
            );
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
