using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class UserSettingsService(
    ILogger<IApplicationUserService> logger,
    UserDataContext userDataContext
) : IUserSettingsService
{
    private readonly ILogger<IApplicationUserService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task<IUserSettingsResponse> ReadUserSettingsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        // Ensure that the user has settings initialized
        if (userData.UserSettings == null)
        {
            var userSettings = new UserSettings { UserID = userData.Id };

            userData.UserSettings = userSettings;
            _userDataContext.UserSettings.Add(userSettings);
            await _userDataContext.SaveChangesAsync();
        }

        return new UserSettingsResponse(userData.UserSettings);
    }

    public async Task UpdateUserSettingsAsync(
        Guid userGuid,
        IUserSettingsUpdateRequest userSettingsUpdateRequest
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var userSettings = userData.UserSettings;

        if (userSettings == null)
        {
            _logger.LogError("User settings not found for user with ID {UserId}", userGuid);
            throw new BudgetBoardServiceException("User settings not found.");
        }

        userSettings.Currency = userSettingsUpdateRequest.Currency;

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.UserSettings)
                .ToListAsync();
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
