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

/// <inheritdoc />
public class AccountService(
    ILogger<IAccountService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAccountService
{
    private readonly ILogger<IAccountService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateAccountAsync(Guid userGuid, IAccountCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (userData.Accounts.Any(a => request.SyncID != null && a.SyncID == request.SyncID))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["DuplicateSyncIDLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["DuplicateSyncIDError"]);
        }

        if (!userData.Institutions.Any(i => i.ID == request.InstitutionID))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidInstitutionIDLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidInstitutionIDError"]);
        }

        var institution = userData.Institutions.Single(i => i.ID == request.InstitutionID);

        // Creating an account under a deleted institution should restore the institution.
        if (institution.Deleted.HasValue)
        {
            institution.Deleted = null;
        }

        var newAccount = new Account
        {
            SyncID = request.SyncID,
            Name = request.Name,
            InstitutionID = request.InstitutionID,
            Type = request.Type,
            Subtype = request.Subtype,
            HideTransactions = request.HideTransactions,
            HideAccount = request.HideAccount,
            Source = request.Source ?? AccountSource.Manual,
            UserID = userData.Id,
        };

        _userDataContext.Accounts.Add(newAccount);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAccountResponse>> ReadAccountsAsync(
        Guid userGuid,
        Guid accountGuid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var accountsQuery = userData.Accounts.ToList();

        if (accountGuid != default)
        {
            accountsQuery = [.. accountsQuery.Where(a => a.ID == accountGuid)];
            if (accountsQuery.Count == 0)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["AccountNotFoundLog"]);
                throw new BudgetBoardServiceException(_responseLocalizer["AccountNotFoundError"]);
            }
        }

        return accountsQuery.OrderBy(a => a.Index).Select(a => new AccountResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAccountAsync(Guid userGuid, IAccountUpdateRequest editedAccount)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == editedAccount.ID);
        if (account == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AccountEditNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AccountEditNotFoundError"]);
        }

        if (string.IsNullOrEmpty(editedAccount.Name))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AccountEditEmptyNameLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AccountEditEmptyNameError"]);
        }

        _userDataContext.Entry(account).CurrentValues.SetValues(editedAccount);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAccountAsync(
        Guid userGuid,
        Guid accountGuid,
        bool deleteTransactions = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountGuid);
        if (account == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AccountDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AccountDeleteNotFoundError"]);
        }

        var now = _nowProvider.UtcNow;
        account.Deleted = now;

        if (deleteTransactions)
        {
            foreach (var transaction in account.Transactions)
            {
                transaction.Deleted = now;
            }
        }

        if (account.Institution?.Accounts.All(a => a.Deleted != null) ?? false)
        {
            account.Institution.Deleted = now;
            account.Institution.Index = 0;
        }

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreAccountAsync(
        Guid userGuid,
        Guid accountGuid,
        bool restoreTransactions = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountGuid);
        if (account == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AccountRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["AccountRestoreNotFoundError"]
            );
        }

        account.Deleted = null;

        if (restoreTransactions)
        {
            foreach (var transaction in account.Transactions)
            {
                transaction.Deleted = null;
            }
        }

        if (account.Institution != null)
        {
            account.Institution.Deleted = null;
        }

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task OrderAccountsAsync(
        Guid userGuid,
        IEnumerable<IAccountIndexRequest> orderedAccounts
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        foreach (var orderedAccount in orderedAccounts)
        {
            var account = userData.Accounts.FirstOrDefault(a => a.ID == orderedAccount.ID);
            if (account == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["AccountOrderNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["AccountOrderNotFoundError"]
                );
            }

            account.Index = orderedAccount.Index;
        }

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            var users = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Institution)
                .Include(u => u.Institutions)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
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
