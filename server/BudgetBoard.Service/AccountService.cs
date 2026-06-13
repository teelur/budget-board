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
    ITransactionService transactionService,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAccountService
{
    /// <inheritdoc />
    public async Task CreateAccountAsync(Guid userGuid, IAccountCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var institution = userData.Institutions.SingleOrDefault(i => i.ID == request.InstitutionID);
        if (institution == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidInstitutionIDLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidInstitutionIDError"]);
        }

        // Creating an account under a deleted institution should restore the institution.
        institution.Deleted = null;

        var newAccount = new Account
        {
            Name = request.Name,
            Source = AccountSource.Manual,
            InstitutionID = request.InstitutionID,
            UserID = userData.Id,
        };

        userDataContext.Accounts.Add(newAccount);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAccountResponse>> ReadAccountsAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var accounts = userData.Accounts.ToList();
        return accounts.OrderBy(a => a.Index).Select(a => new AccountResponse(a)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAccountAsync(Guid userGuid, IAccountUpdateRequest editedAccount)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == editedAccount.ID);
        if (account == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountEditNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AccountEditNotFoundError"]);
        }

        account.Name = editedAccount.Name ?? account.Name;
        account.Type = editedAccount.Type ?? account.Type;
        account.HideTransactions = editedAccount.HideTransactions ?? account.HideTransactions;
        account.HideAccount = editedAccount.HideAccount ?? account.HideAccount;
        account.InterestRate = editedAccount.InterestRate ?? account.InterestRate;
        if (editedAccount.Source != null)
        {
            if (!AccountSource.IsValid(editedAccount.Source))
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidAccountSourceError"]
                );
            }

            account.Source = editedAccount.Source;
        }

        await userDataContext.SaveChangesAsync();
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
            logger.LogError("{LogMessage}", logLocalizer["AccountDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AccountDeleteNotFoundError"]);
        }

        var now = nowProvider.Now;
        account.Deleted = now;
        account.Type = string.Empty;
        account.Source = AccountSource.Manual;

        if (deleteTransactions)
        {
            await transactionService.DeleteTransactionBatchAsync(
                userGuid,
                account.Transactions.Select(t => t.ID),
                true
            );
        }

        if (account.Institution?.Accounts.All(a => a.Deleted != null) ?? false)
        {
            account.Institution.Deleted = now;
            account.Institution.Index = 0;
        }

        var lunchFlowAccount = await userDataContext.LunchFlowAccounts.FirstOrDefaultAsync(a =>
            a.LinkedAccountId == accountGuid
        );
        if (lunchFlowAccount != null)
        {
            lunchFlowAccount.LinkedAccountId = null;
            lunchFlowAccount.LastSync = null;
        }

        var simpleFinAccount = await userDataContext.SimpleFinAccounts.FirstOrDefaultAsync(a =>
            a.LinkedAccountId == accountGuid
        );
        if (simpleFinAccount != null)
        {
            simpleFinAccount.LinkedAccountId = null;
            simpleFinAccount.LastSync = null;
        }

        await userDataContext.SaveChangesAsync();
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
            logger.LogError("{LogMessage}", logLocalizer["AccountRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AccountRestoreNotFoundError"]);
        }

        account.Deleted = null;
        account.Institution?.Deleted = null;

        if (restoreTransactions)
        {
            await transactionService.RestoreTransactionBatchAsync(
                userGuid,
                account.Transactions.Where(t => t.Deleted != null).Select(t => t.ID),
                true
            );
        }

        await userDataContext.SaveChangesAsync();
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
                logger.LogError("{LogMessage}", logLocalizer["AccountOrderNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["AccountOrderNotFoundError"]
                );
            }

            account.Index = orderedAccount.Index;
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task PermanentlyDeleteAccountAsync(Guid userGuid, Guid accountGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountGuid);
        if (account == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountPermanentDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountPermanentDeleteNotFoundError"]
            );
        }

        userDataContext.Transactions.RemoveRange(account.Transactions);
        userDataContext.Balances.RemoveRange(account.Balances);
        userDataContext.Accounts.Remove(account);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        try
        {
            var foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Institution)
                .Include(u => u.Institutions)
                .Include(u => u.LunchFlowAccounts)
                .Include(u => u.SimpleFinAccounts)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));

            if (foundUser == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
                throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
            }
            return foundUser;
        }
        catch (BudgetBoardServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }
    }
}
