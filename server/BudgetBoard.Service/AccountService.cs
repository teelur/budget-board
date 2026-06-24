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
    public async Task UpdateAccountAsync(Guid userGuid, IAccountUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = GetAccountById(userData, request.ID);

        if (request.Name.IsSpecified && !string.IsNullOrWhiteSpace(request.Name.Value))
        {
            account.Name = request.Name.Value;
        }
        if (request.Type.IsSpecified)
        {
            account.Type = request.Type.Value ?? string.Empty;
        }
        if (request.HideTransactions.IsSpecified)
        {
            account.HideTransactions = request.HideTransactions.Value;
        }
        if (request.HideAccount.IsSpecified)
        {
            account.HideAccount = request.HideAccount.Value;
        }
        if (request.InterestRate.IsSpecified)
        {
            account.InterestRate = request.InterestRate.Value;
        }
        if (request.Source.IsSpecified)
        {
            if (
                string.IsNullOrEmpty(request.Source.Value)
                || !AccountSource.IsValid(request.Source.Value ?? string.Empty)
            )
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["InvalidAccountSourceError"]
                );
            }
            account.Source = request.Source.Value!;
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
        var account = GetAccountById(userData, accountGuid);

        var utcNow = nowProvider.UtcNow;
        account.Deleted = utcNow;
        account.Type = string.Empty;

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
            account.Institution.Deleted = utcNow;
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

        // It's important that this gets set after we delete the transactions, since the
        // source determines whether transactions affect the account balance.
        account.Source = AccountSource.Manual;

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
        var account = GetAccountById(userData, accountGuid);

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
            var account = GetAccountById(userData, orderedAccount.ID);
            account.Index = orderedAccount.Index;
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task PermanentlyDeleteAccountAsync(Guid userGuid, Guid accountGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = GetAccountById(userData, accountGuid);
        if (account.Deleted == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountPermanentDeleteNotDeletedLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["AccountPermanentDeleteNotDeletedError"]
            );
        }

        userDataContext.Transactions.RemoveRange(account.Transactions);
        userDataContext.Balances.RemoveRange(account.Balances);
        userDataContext.Accounts.Remove(account);
        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
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
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Balances)
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Institution)
                    .Include(u => u.Institutions)
                    .Include(u => u.LunchFlowAccounts)
                    .Include(u => u.SimpleFinAccounts)
        );
    }

    private Account GetAccountById(ApplicationUser userData, Guid accountId)
    {
        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountId);
        if (account == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AccountNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AccountNotFoundError"]);
        }

        return account;
    }
}
