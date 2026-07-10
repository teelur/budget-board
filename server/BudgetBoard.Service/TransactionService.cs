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

public class TransactionService(
    ILogger<ITransactionService> logger,
    UserDataContext userDataContext,
    INowProvider nowProvider,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ITransactionService
{
    /// <inheritdoc />
    public async Task CreateTransactionAsync(
        ApplicationUser userData,
        ITransactionCreateRequest request,
        IEnumerable<ITransactionCategory>? allCategories = null,
        AutomaticTransactionCategorizerHelper? autoCategorizer = null
    )
    {
        var account = GetAccountByID(userData, request.AccountID);

        var newTransaction = new Transaction
        {
            SyncID = request.SyncID,
            Amount = request.Amount,
            Date = request.Date,
            Category = request.Category,
            Subcategory = request.Subcategory,
            MerchantName = request.MerchantName,
            Source = request.Source ?? TransactionSource.Manual.Value,
            AccountID = request.AccountID,
            Account = account,
        };
        AutoCategorizeTransaction(userData, newTransaction, allCategories, autoCategorizer);

        userDataContext.Transactions.Add(newTransaction);

        // Manual accounts need to manually update the balance
        if (account.Source == AccountSource.Manual)
        {
            UpdateBalancesForNewTransaction(account, request);
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task CreateTransactionAsync(
        Guid userGuid,
        ITransactionCreateRequest request,
        IEnumerable<ITransactionCategory>? allCategories = null,
        AutomaticTransactionCategorizerHelper? autoCategorizer = null
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        await CreateTransactionAsync(userData, request, allCategories, autoCategorizer);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionResponse>> ReadTransactionsAsync(
        Guid userGuid,
        int? year,
        int? month,
        bool getHidden
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var transactions = userData
            .Accounts.SelectMany(t => t.Transactions)
            .Where(t => getHidden || t.Account!.HideTransactions is not true);

        if (year != null)
        {
            transactions = transactions.Where(t => t.Date.Year == year);
        }
        if (month != null)
        {
            transactions = transactions.Where(t => t.Date.Month == month);
        }

        return transactions
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionResponse(t))
            .ToList();
    }

    /// <inheritdoc />
    public async Task UpdateTransactionsAsync(
        Guid userGuid,
        IEnumerable<ITransactionUpdateRequest> requests
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        foreach (var request in requests)
        {
            var transaction = GetTransactionByID(userData, request.ID);
            var originalAmount = transaction.Amount;
            var originalDate = transaction.Date;
            var finalAmount = request.Amount ?? originalAmount;
            var finalDate = request.Date ?? originalDate;

            if (request.Amount.HasValue)
            {
                transaction.Amount = finalAmount;
            }
            if (request.Date.HasValue)
            {
                transaction.Date = finalDate;
            }
            if (request.Category.IsSpecified)
            {
                transaction.Category = request.Category.Value;
            }
            if (request.Subcategory.IsSpecified)
            {
                transaction.Subcategory = request.Subcategory.Value;
            }
            if (request.MerchantName.IsSpecified)
            {
                transaction.MerchantName = request.MerchantName.Value;
            }

            if (transaction.Account?.Source == AccountSource.Manual)
            {
                var balancesAfterOriginal = transaction
                    .Account.Balances.Where(b => b.Date >= originalDate)
                    .ToList();
                foreach (var balance in balancesAfterOriginal)
                {
                    balance.Amount -= originalAmount;
                }

                var balancesAfterEdited = transaction
                    .Account.Balances.Where(b => b.Date >= finalDate)
                    .ToList();
                foreach (var balance in balancesAfterEdited)
                {
                    balance.Amount += finalAmount;
                }
            }
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteTransactionAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == guid);
        if (transaction == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionDeleteNotFoundError"]
            );
        }

        MarkTransactionAsDeleted(transaction);

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteTransactionBatchAsync(
        Guid userGuid,
        IEnumerable<Guid> guids,
        bool deferSave = false
    )
    {
        var guidList = guids.ToList();
        var duplicateIds = guidList
            .GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateIds.Count > 0)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionBatchDeleteDuplicateIdsLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionBatchDeleteDuplicateIdsError"]
            );
        }

        var userData = await GetCurrentUserAsync(userGuid);
        var allTransactions = userData.Accounts.SelectMany(a => a.Transactions).ToList();

        foreach (var guid in guidList)
        {
            var transaction = allTransactions.FirstOrDefault(t => t.ID == guid);
            if (transaction == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["TransactionDeleteNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionDeleteNotFoundError"]
                );
            }

            MarkTransactionAsDeleted(transaction);
        }

        if (!deferSave)
        {
            await userDataContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task RestoreTransactionAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == guid);
        if (transaction == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionRestoreNotFoundError"]
            );
        }

        transaction.Deleted = null;
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreTransactionBatchAsync(
        Guid userGuid,
        IEnumerable<Guid> guids,
        bool deferSave = false
    )
    {
        var guidList = guids.ToList();
        var duplicateIds = guidList
            .GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicateIds.Count > 0)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionBatchRestoreDuplicateIdsLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionBatchRestoreDuplicateIdsError"]
            );
        }

        var userData = await GetCurrentUserAsync(userGuid);
        var allTransactions = userData.Accounts.SelectMany(a => a.Transactions).ToList();

        foreach (var guid in guidList)
        {
            var transaction = allTransactions.FirstOrDefault(t => t.ID == guid);
            if (transaction == null)
            {
                logger.LogError("{LogMessage}", logLocalizer["TransactionRestoreNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionRestoreNotFoundError"]
                );
            }

            transaction.Deleted = null;
        }

        if (!deferSave)
        {
            await userDataContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task SplitTransactionAsync(
        Guid userGuid,
        ITransactionSplitRequest transactionSplitRequest
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == transactionSplitRequest.ID);
        if (transaction == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionSplitNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionSplitNotFoundError"]
            );
        }

        if (Math.Abs(transaction.Amount) <= Math.Abs(transactionSplitRequest.Amount))
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionSplitInvalidAmountLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionSplitInvalidAmountError"]
            );
        }

        var splitTransaction = new Transaction
        {
            SyncID = transaction.SyncID,
            Amount = transactionSplitRequest.Amount,
            Date = transaction.Date,
            Category = transactionSplitRequest.Category,
            Subcategory = transactionSplitRequest.Subcategory,
            MerchantName = transaction.MerchantName,
            Source = transaction.Source ?? TransactionSource.Manual.Value,
            AccountID = transaction.AccountID,
        };

        transaction.Amount -= transactionSplitRequest.Amount;

        userDataContext.Transactions.Add(splitTransaction);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ImportTransactionsAsync(Guid userGuid, ITransactionImportRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var transactions = request.Transactions;
        var accountNameToIDMap = request.AccountNameToIDMap;

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var autoCategorizer =
            await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
                userDataContext,
                userData
            );

        foreach (var transaction in transactions)
        {
            var account = userData.Accounts.FirstOrDefault(a =>
                a.ID
                == accountNameToIDMap
                    .FirstOrDefault(a =>
                        a.AccountName.Equals(
                            transaction.Account,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
                    ?.AccountID
            );
            if (account == null)
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionImportAccountNotFoundLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionImportAccountNotFoundError"]
                );
            }

            string matchedCategory =
                allCategories
                    .FirstOrDefault(c =>
                        c.Value.Equals(
                            transaction.Category,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
                    ?.Value ?? string.Empty;

            var newTransaction = new TransactionCreateRequest
            {
                SyncID = string.Empty,
                Amount = transaction.Amount ?? 0,
                Date = transaction.Date ?? DateOnly.FromDateTime(nowProvider.Now),
                MerchantName = transaction.MerchantName,
                Source = TransactionSource.Manual.Value,
                AccountID = account.ID,
            };

            (newTransaction.Category, newTransaction.Subcategory) =
                TransactionCategoriesHelpers.GetFullCategory(matchedCategory, allCategories);

            await CreateTransactionAsync(userData, newTransaction, allCategories, autoCategorizer);
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
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Balances)
                    .Include(u => u.UserSettings)
                    .Include(u => u.TransactionCategories)
        );
    }

    private Transaction GetTransactionByID(ApplicationUser userData, Guid transactionID)
    {
        var transaction = userData
            .Accounts.SelectMany(a => a.Transactions)
            .FirstOrDefault(t => t.ID == transactionID);
        if (transaction == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["TransactionNotFoundError"]);
        }
        return transaction;
    }

    private Account GetAccountByID(ApplicationUser userData, Guid accountID)
    {
        var account = userData.Accounts.FirstOrDefault(a => a.ID == accountID);
        if (account == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionAccountNotFoundError"]
            );
        }

        return account;
    }

    private void AutoCategorizeTransaction(
        ApplicationUser userData,
        Transaction transaction,
        IEnumerable<ITransactionCategory>? allCategories,
        AutomaticTransactionCategorizerHelper? autoCategorizer
    )
    {
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

    private void UpdateBalancesForNewTransaction(
        Account account,
        ITransactionCreateRequest transaction
    )
    {
        var currentBalance = account
            .Balances.Where(b => b.Date <= transaction.Date)
            .OrderByDescending(b => b.Date)
            .FirstOrDefault();

        // First, add the new balance for the new transaction if no balance exists for that date.
        if (currentBalance == null || currentBalance.Date != transaction.Date)
        {
            var newBalance = new Balance
            {
                Amount = currentBalance?.Amount ?? 0,
                Date = transaction.Date,
                AccountID = account.ID,
            };

            userDataContext.Balances.Add(newBalance);
        }

        // Then, update all following balances to include the new transaction.
        var balancesAfterNew = account.Balances.Where(b => b.Date >= transaction.Date).ToList();
        foreach (var balance in balancesAfterNew)
        {
            balance.Amount += transaction.Amount;
        }
    }

    private void MarkTransactionAsDeleted(Transaction transaction)
    {
        transaction.Deleted = nowProvider.UtcNow;
        transaction.Category = null;
        transaction.Subcategory = null;

        Account account = transaction.Account!;

        if (account.Source == AccountSource.Manual)
        {
            var balancesAfterDeleted = account
                .Balances.Where(b => b.Date >= transaction.Date)
                .ToList();
            foreach (var balance in balancesAfterDeleted)
            {
                balance.Amount -= transaction.Amount;
            }
        }
    }
}
