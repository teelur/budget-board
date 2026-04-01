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
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var userSettings = userData.UserSettings;
        if (userSettings == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["UserSettingsNotFoundError"]);
        }

        var trainingTransactions = userData.Accounts
            .Where(a => a.Deleted is null) // Filter out deleted accounts
            .Select(a => a.Transactions)
            .SelectMany(c => c)
            .Where(t => t.Deleted is null && t.Category is not null && !t.Category.Equals(string.Empty)); // Filter out deleted transactions or those without category
        if (request.StartDate is not null)
        {
            trainingTransactions = trainingTransactions.Where(t =>
                DateOnly.FromDateTime(t.Date) >= request.StartDate
            );
        }
        if (request.EndDate is not null)
        {
            trainingTransactions = trainingTransactions.Where(t =>
                DateOnly.FromDateTime(t.Date) <= request.EndDate
            );
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
        userSettings.AutoCategorizerModelStartDate = DateOnly.FromDateTime(
            trainingTransactions.Min(t => t.Date)
        );
        userSettings.AutoCategorizerModelEndDate = DateOnly.FromDateTime(
            trainingTransactions.Max(t => t.Date)
        );

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
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
