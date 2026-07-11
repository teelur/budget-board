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

public class AutomaticTransactionCategorizerService(
    ILogger<IAutomaticTransactionCategorizerService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAutomaticTransactionCategorizerService
{
    /// <inheritdoc />
    public async Task TrainCategorizerAsync(Guid userGuid, ITrainAutoCategorizerRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var userSettings = userData.UserSettings;
        if (userSettings == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["UserSettingsNotFoundError"]);
        }

        var trainingTransactions = userData
            .Accounts.Where(a => a.Deleted is null) // Filter out deleted accounts
            .Select(a => a.Transactions)
            .SelectMany(c => c)
            .Where(t =>
                t.Deleted is null && t.Category is not null && !t.Category.Equals(string.Empty)
            ); // Filter out deleted transactions or those without category
        if (request.StartDate is not null)
        {
            trainingTransactions = trainingTransactions.Where(t => t.Date >= request.StartDate);
        }
        if (request.EndDate is not null)
        {
            trainingTransactions = trainingTransactions.Where(t => t.Date <= request.EndDate);
        }

        if (!trainingTransactions.Any())
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["AutoCategorizerTrainingNoTransactions"]
            );
        }

        var mlModel = AutomaticTransactionCategorizerHelper.Train(trainingTransactions);

        // The ML model is serialized and stored as a large object in the database. If the user already has a model, it will be overwritten.
        long objectId = userSettings.AutoCategorizerModelOID ?? 0;
        objectId = await userDataContext.WriteLargeObjectAsync(objectId, mlModel);

        userSettings.AutoCategorizerModelOID = objectId;
        userSettings.AutoCategorizerLastTrained = DateOnly.FromDateTime(nowProvider.Now);
        userSettings.AutoCategorizerModelStartDate = trainingTransactions.Min(t => t.Date);
        userSettings.AutoCategorizerModelEndDate = trainingTransactions.Max(t => t.Date);

        await userDataContext.SaveChangesAsync();
    }

    public async Task AutoCategorizeTransactionAsync(Guid userGuid, Transaction transaction)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var autoCategorizer =
            await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
                userDataContext,
                userData
            );
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        if (
            autoCategorizer is not null
            && allCategories is not null
            && transaction.MerchantName is not null
            && transaction.MerchantName != string.Empty
        )
        {
            var (PredictionCategory, PredictionProbability) = autoCategorizer.PredictCategory(
                transaction
            );

            logger.LogInformation(
                "{LogMessage}",
                logLocalizer[
                    "AutoCategorizerPredictionLog",
                    PredictionCategory,
                    PredictionProbability,
                    transaction.MerchantName,
                    transaction.Account?.Name ?? "Unknown Account",
                    transaction.Amount
                ]
            );

            if (
                PredictionProbability
                >= (userData.UserSettings?.AutoCategorizerMinimumProbabilityPercentage ?? 70) / 100f
            )
            {
                (transaction.Category, transaction.Subcategory) =
                    TransactionCategoriesHelpers.GetFullCategory(PredictionCategory, allCategories);
            }
            else
            {
                logger.LogInformation(
                    "{LogMessage}",
                    logLocalizer[
                        "AutoCategorizerPredictionBelowThresholdLog",
                        PredictionCategory,
                        PredictionProbability,
                        userData.UserSettings?.AutoCategorizerMinimumProbabilityPercentage ?? 70,
                        transaction.MerchantName,
                        transaction.Amount
                    ]
                );
            }
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
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Transactions)
                    .Include(u => u.UserSettings)
        );
    }
}
