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
    private readonly ILogger<IAutomaticTransactionCategorizerService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task TrainCategorizerAsync(Guid userGuid, ITrainAutoCategorizerRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var userSettings = userData.UserSettings;
        if (userSettings == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["UserSettingsNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["UserSettingsNotFoundError"]);
        }

        var trainingTransactions = userData.Accounts.Select(a => a.Transactions).SelectMany(c => c);
        if (request.StartDate is not null)
        {
            trainingTransactions = trainingTransactions.Where(t => DateOnly.FromDateTime(t.Date) >= request.StartDate);
        }
        if (request.EndDate is not null)
        {
            trainingTransactions = trainingTransactions.Where(t => DateOnly.FromDateTime(t.Date) <= request.EndDate);
        }

        if (trainingTransactions.Count() == 0)
        {
            throw new BudgetBoardServiceException(_responseLocalizer["AutoCategorizerTrainingNoTransactions"]);
        }

        var mlModel = AutomaticTransactionCategorizer.Train(trainingTransactions);

        // Store the ML model
        long objectId = userSettings.AutoCategorizerModelOID ?? 0;
        objectId = await _userDataContext.WriteLargeObjectAsync(objectId, mlModel);

        // Update user settings
        userSettings.AutoCategorizerModelOID = objectId;
        userSettings.AutoCategorizerLastTrained = DateOnly.FromDateTime(nowProvider.Now);
        userSettings.AutoCategorizerModelStartDate = DateOnly.FromDateTime(trainingTransactions.Min(t => t.Date));
        userSettings.AutoCategorizerModelEndDate = DateOnly.FromDateTime(trainingTransactions.Max(t => t.Date));

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
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
