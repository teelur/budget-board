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
        var userData = await GetCurrentUserAsync(userGuid);

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
        var userData = await GetCurrentUserAsync(userGuid);

        var userSettings = userData.UserSettings;
        if (userSettings == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["UserSettingsNotFoundError"]);
        }

        HandleCurrencyChange();
        HandleLanguageChange();
        HandleDateFormatChange();
        HandleBudgetWarningThresholdChange();
        HandleForceSyncLookbackMonthsChange();
        HandleDisableBuiltInTransactionCategoriesChange();
        HandleDisableBuiltInAccountTypesChange();
        HandleDisableBuiltInAssetTypesChange();
        HandleEnableAutoCategorizerChange();
        HandleAutoCategorizerMinimumProbabilityPercentageChange();

        await userDataContext.SaveChangesAsync();

        void HandleCurrencyChange()
        {
            if (
                string.IsNullOrEmpty(request.Currency)
                || request.Currency.Equals(
                    userSettings.Currency,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                return;
            }

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

        void HandleLanguageChange()
        {
            if (
                string.IsNullOrEmpty(request.Language)
                || request.Language.Equals(
                    userSettings.Language,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                return;
            }

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

        void HandleDateFormatChange()
        {
            if (
                string.IsNullOrEmpty(request.DateFormat)
                || request.DateFormat.Equals(
                    userSettings.DateFormat,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
            {
                return;
            }

            var isValidDateFormat = LocalizationHelpers.IsValidDateFormat(request.DateFormat);
            if (!isValidDateFormat)
            {
                logger.LogError("{LogMessage}", logLocalizer["InvalidDateFormatLog"]);
                throw new BudgetBoardServiceException(responseLocalizer["InvalidDateFormatError"]);
            }

            userSettings.DateFormat = request.DateFormat;
        }

        void HandleBudgetWarningThresholdChange()
        {
            if (
                !request.BudgetWarningThreshold.HasValue
                || userSettings.BudgetWarningThreshold == request.BudgetWarningThreshold.Value
            )
            {
                return;
            }

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

        void HandleForceSyncLookbackMonthsChange()
        {
            if (
                !request.ForceSyncLookbackMonths.HasValue
                || userSettings.ForceSyncLookbackMonths == request.ForceSyncLookbackMonths.Value
            )
            {
                return;
            }

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

        void HandleDisableBuiltInTransactionCategoriesChange()
        {
            if (
                !request.DisableBuiltInTransactionCategories.HasValue
                || userSettings.DisableBuiltInTransactionCategories
                    == request.DisableBuiltInTransactionCategories.Value
            )
            {
                return;
            }

            userSettings.DisableBuiltInTransactionCategories = request
                .DisableBuiltInTransactionCategories
                .Value;
        }

        void HandleDisableBuiltInAccountTypesChange()
        {
            if (
                !request.DisableBuiltInAccountTypes.HasValue
                || userSettings.DisableBuiltInAccountTypes
                    == request.DisableBuiltInAccountTypes.Value
            )
            {
                return;
            }

            var builtInTypeValues = AccountTypeConstants
                .DefaultAccountTypes.Select(at => at.Value.ToLower())
                .ToHashSet();
            if (request.DisableBuiltInAccountTypes.Value)
            {
                var accountUsesBuiltInType = userData.Accounts.Any(a =>
                    builtInTypeValues.Contains(a.Type)
                );
                var customTypeHasBuiltInParent = userData.AccountTypes.Any(cat =>
                    builtInTypeValues.Contains(cat.Parent)
                );
                if (accountUsesBuiltInType || customTypeHasBuiltInParent)
                {
                    logger.LogError(
                        "{LogMessage}",
                        logLocalizer["DisableBuiltInAccountTypesInUseLog"]
                    );
                    throw new BudgetBoardServiceException(
                        responseLocalizer["DisableBuiltInAccountTypesInUseError"]
                    );
                }
            }
            else
            {
                var hasConflictingCustomAccountTypes = userData.AccountTypes.Any(cat =>
                    builtInTypeValues.Contains(cat.Value.ToLower())
                );
                if (hasConflictingCustomAccountTypes)
                {
                    logger.LogError(
                        "{LogMessage}",
                        logLocalizer["EnableBuiltInAccountTypesConflictLog"]
                    );
                    throw new BudgetBoardServiceException(
                        responseLocalizer["EnableBuiltInAccountTypesConflictError"]
                    );
                }
            }

            userSettings.DisableBuiltInAccountTypes = request.DisableBuiltInAccountTypes.Value;
        }

        void HandleDisableBuiltInAssetTypesChange()
        {
            if (
                !request.DisableBuiltInAssetTypes.HasValue
                || userSettings.DisableBuiltInAssetTypes == request.DisableBuiltInAssetTypes.Value
            )
            {
                return;
            }

            var builtInTypeValues = AssetTypeConstants
                .DefaultAssetTypes.Select(at => at.Value.ToLower())
                .ToHashSet();
            if (request.DisableBuiltInAssetTypes.Value)
            {
                var assetsUsesBuiltInType = userData.Assets.Any(a =>
                    builtInTypeValues.Contains(a.Type.ToLower())
                );
                var customTypeHasBuiltInParent = userData.AssetTypes.Any(at =>
                    builtInTypeValues.Contains(at.Parent.ToLower())
                );
                if (assetsUsesBuiltInType || customTypeHasBuiltInParent)
                {
                    logger.LogError(
                        "{LogMessage}",
                        logLocalizer["DisableBuiltInAssetTypesInUseLog"]
                    );
                    throw new BudgetBoardServiceException(
                        responseLocalizer["DisableBuiltInAssetTypesInUseError"]
                    );
                }
            }
            else
            {
                // Built-in types cannot be re-enabled if the user has custom asset types that conflict with the built-in types.
                var hasConflictingCustomAssetTypes = userData.AssetTypes.Any(at =>
                    builtInTypeValues.Contains(at.Value.ToLower())
                );
                if (hasConflictingCustomAssetTypes)
                {
                    logger.LogError(
                        "{LogMessage}",
                        logLocalizer["EnableBuiltInAssetTypesConflictLog"]
                    );
                    throw new BudgetBoardServiceException(
                        responseLocalizer["EnableBuiltInAssetTypesConflictError"]
                    );
                }
            }

            userSettings.DisableBuiltInAssetTypes = request.DisableBuiltInAssetTypes.Value;
        }

        void HandleEnableAutoCategorizerChange()
        {
            if (
                !request.EnableAutoCategorizer.HasValue
                || userSettings.EnableAutoCategorizer == request.EnableAutoCategorizer.Value
            )
            {
                return;
            }

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

        void HandleAutoCategorizerMinimumProbabilityPercentageChange()
        {
            if (
                !request.AutoCategorizerMinimumProbabilityPercentage.HasValue
                || userSettings.AutoCategorizerMinimumProbabilityPercentage
                    == request.AutoCategorizerMinimumProbabilityPercentage.Value
            )
            {
                return;
            }

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
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users =>
                users
                    .Include(u => u.UserSettings)
                    .Include(u => u.Accounts)
                    .Include(u => u.AccountTypes)
                    .Include(u => u.Assets)
                    .Include(u => u.AssetTypes)
        );
    }
}
