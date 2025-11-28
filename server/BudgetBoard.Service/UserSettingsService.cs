using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class UserSettingsService(
    ILogger<IApplicationUserService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IUserSettingsService
{
    private readonly ILogger<IApplicationUserService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task UpdateUserSettingsAsync(Guid userGuid, IUserSettingsUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var userSettings = userData.UserSettings;
        if (userSettings == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["UserSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["UserSettingsNotFoundError"]);
        }

        if (request.Currency != null)
        {
            var isValidCurrency = System
                .Globalization.CultureInfo.GetCultures(
                    System.Globalization.CultureTypes.SpecificCultures
                )
                .Select(c => new System.Globalization.RegionInfo(c.Name))
                .Any(r => r.ISOCurrencySymbol == request.Currency);

            if (!isValidCurrency)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["InvalidCurrencyCodeLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["InvalidCurrencyCodeError"]
                );
            }

            userSettings.Currency = request.Currency;
        }

        if (request.BudgetWarningThreshold != null)
        {
            if (request.BudgetWarningThreshold < 0 || request.BudgetWarningThreshold > 100)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["InvalidBudgetWarningThresholdLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["InvalidBudgetWarningThresholdError"]
                );
            }

            userSettings.BudgetWarningThreshold = (int)request.BudgetWarningThreshold;
        }

        if (request.ForceSyncLookbackMonths != null)
        {
            if (request.ForceSyncLookbackMonths < 0 || request.ForceSyncLookbackMonths > 12)
            {
                _logger.LogError(
                    "{LogMessage}",
                    _logLocalizer["InvalidForceSyncLookbackMonthsLog"]
                );
                throw new BudgetBoardServiceException(
                    _responseLocalizer["InvalidForceSyncLookbackMonthsError"]
                );
            }
            userSettings.ForceSyncLookbackMonths = (int)request.ForceSyncLookbackMonths;
        }

        if (request.DisableBuiltInTransactionCategories != null)
        {
            userSettings.DisableBuiltInTransactionCategories = (bool)
                request.DisableBuiltInTransactionCategories;
        }

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.UserSettings)
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
