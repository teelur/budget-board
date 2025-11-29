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

public class BalanceService(
    ILogger<IBalanceService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IBalanceService
{
    private readonly ILogger<IBalanceService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateBalancesAsync(Guid userGuid, IBalanceCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == request.AccountID);
        if (account == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["BalanceAccountCreateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["BalanceAccountCreateNotFoundError"]
            );
        }

        var newBalance = new Balance
        {
            DateTime = request.DateTime,
            Amount = request.Amount,
            AccountID = request.AccountID,
        };

        _userDataContext.Balances.Add(newBalance);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IBalanceResponse>> ReadBalancesAsync(
        Guid userGuid,
        Guid accountId
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountId);
        if (account == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["BalanceAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["BalanceAccountNotFoundError"]
            );
        }

        return account.Balances.Select(b => new BalanceResponse(b)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateBalanceAsync(Guid userGuid, IBalanceUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == request.ID);
        if (balance == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["BalanceUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["BalanceUpdateNotFoundError"]);
        }

        _userDataContext.Entry(balance).CurrentValues.SetValues(request);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteBalanceAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == guid);
        if (balance == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["BalanceDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["BalanceDeleteNotFoundError"]);
        }

        balance.Deleted = _nowProvider.UtcNow;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreBalanceAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var balance = userData
            .Accounts.SelectMany(a => a.Balances)
            .FirstOrDefault(b => b.ID == guid);
        if (balance == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["BalanceRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["BalanceRestoreNotFoundError"]
            );
        }

        balance.Deleted = null;
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
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
