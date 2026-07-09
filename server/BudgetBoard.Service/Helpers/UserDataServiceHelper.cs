using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service.Helpers;

public static class UserDataServiceHelper
{
    public static async Task<ApplicationUser> GetCurrentUserAsync(
        UserDataContext userDataContext,
        ILogger logger,
        IStringLocalizer<LogStrings> logLocalizer,
        IStringLocalizer<ResponseStrings> responseLocalizer,
        Guid id,
        Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> includeQuery
    )
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await includeQuery(userDataContext.ApplicationUsers)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{LogMessage}",
                logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
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
