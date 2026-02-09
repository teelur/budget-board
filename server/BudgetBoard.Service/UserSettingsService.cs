using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class UserSettingsService(
    ILogger<IUserSettingsService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IUserSettingsService
{
    /// <inheritdoc />
    public async Task<IUserSettingsResponse> ReadUserSettingsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        // Ensure that the user has settings initialized
        if (userData.UserSettings == null)
        {
            var userSettings = new UserSettings { UserID = userData.Id };

            userData.UserSettings = userSettings;

            userDataContext.UserSettings.Add(userSettings);
            await userDataContext.SaveChangesAsync();
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
            logger.LogError("{LogMessage}", logLocalizer["UserSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["UserSettingsNotFoundError"]);
        }

        if (!string.IsNullOrEmpty(request.Currency))
        {
            var isValidCurrency = LocalizationHelpers.CurrencyCodes.Contains(request.Currency);
            if (!isValidCurrency)
            {
                logger.LogError("{LogMessage}", logLocalizer["InvalidCurrencyCodeLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidCurrencyCodeError"]
                );
            }

            userSettings.Currency = request.Currency;
        }

        if (!string.IsNullOrEmpty(request.Language))
        {
            var isValidLanguage = SupportedLanguages.AllUserLanguageOptions.Contains(
                request.Language.ToLower()
            );
            if (!isValidLanguage)
            {
                logger.LogError("{LogMessage}", logLocalizer["InvalidLanguageCodeLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidLanguageCodeError"]
                );
            }

            userSettings.Language = request.Language.ToLower();
        }

        if (request.BudgetWarningThreshold.HasValue)
        {
            if (
                request.BudgetWarningThreshold.Value < 0
                || request.BudgetWarningThreshold.Value > 100
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["InvalidBudgetWarningThresholdLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidBudgetWarningThresholdError"]
                );
            }

            userSettings.BudgetWarningThreshold = request.BudgetWarningThreshold.Value;
        }

        if (request.ForceSyncLookbackMonths.HasValue)
        {
            if (
                request.ForceSyncLookbackMonths.Value < 0
                || request.ForceSyncLookbackMonths.Value > 12
            )
            {
                logger.LogError("{LogMessage}", logLocalizer["InvalidForceSyncLookbackMonthsLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidForceSyncLookbackMonthsError"]
                );
            }
            userSettings.ForceSyncLookbackMonths = request.ForceSyncLookbackMonths.Value;
        }

        if (request.DisableBuiltInTransactionCategories.HasValue)
        {
            userSettings.DisableBuiltInTransactionCategories = request
                .DisableBuiltInTransactionCategories
                .Value;
        }

        if (request.EnableAutoCategorizer.HasValue)
        {
            // We can only enable auto categorizer if we trained it
            if (request.EnableAutoCategorizer.Value && userSettings.AutoCategorizerModelOID == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["AutoCategorizerNotTrainedLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AutoCategorizerNotTrained"]
                );
            }
            userSettings.EnableAutoCategorizer = request.EnableAutoCategorizer.Value;
        }

        if (request.AutoCategorizerMinimumProbabilityPercentage.HasValue)
        {
            if (
                request.AutoCategorizerMinimumProbabilityPercentage.Value < 0
                || request.AutoCategorizerMinimumProbabilityPercentage.Value > 100
            )
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["InvalidAutoCategorizerMinimumProbabilityPercentageLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidAutoCategorizerMinimumProbabilityPercentageError"]
                );
            }
            userSettings.AutoCategorizerMinimumProbabilityPercentage = request
                .AutoCategorizerMinimumProbabilityPercentage
                .Value;
        }

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.UserSettings)
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
