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
    IAutomaticTransactionCategorizerService automaticTransactionCategorizerService,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ITransactionService
{
    /// <inheritdoc />
    public async Task CreateTransactionAsync(
        Guid userGuid,
        ITransactionCreateRequest request,
        bool deferSave = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        await CreateTransactionAsync(userData, request, deferSave);
    }

    /// <inheritdoc />
    public async Task CreateTransactionAsync(
        ApplicationUser userData,
        ITransactionCreateRequest request,
        bool deferSave = false
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
            Source = request.Source ?? TransactionSource.Manual,
            AccountID = request.AccountID,
            Account = account,
        };
        await automaticTransactionCategorizerService.AutoCategorizeTransactionAsync(
            userData.Id,
            newTransaction
        );

        userDataContext.Transactions.Add(newTransaction);

        // Manual accounts need to manually update the balance
        if (account.Source == AccountSource.Manual)
        {
            UpdateBalancesForNewTransaction(account, request);
        }

        if (!deferSave)
        {
            await userDataContext.SaveChangesAsync();
        }

        void UpdateBalancesForNewTransaction(Account account, ITransactionCreateRequest transaction)
        {
            CreateBalanceForDateIfNotExists(account, transaction.Date);

            var affectedBalances = account.Balances.Where(b => b.Date >= transaction.Date).ToList();
            foreach (var balance in affectedBalances)
            {
                balance.Amount += transaction.Amount;
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionResponse>> ReadTransactionsAsync(
        Guid userGuid,
        int? year,
        int? month,
        bool includeHidden,
        bool includeDeleted
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var transactions = userData.Accounts.SelectMany(t => t.Transactions);

        if (!includeDeleted)
        {
            transactions = transactions.Where(t => t.Deleted == null);
        }

        if (!includeHidden)
        {
            transactions = transactions.Where(t =>
                t.Account!.HideTransactions is false
                && t.Category != TransactionCategoriesConstants.HideFromBudgetsCategory
            );
        }

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
        IEnumerable<ITransactionUpdateRequest> requests,
        bool deferSave = false
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

            UpdateBalancesForEditedTransaction(
                transaction,
                originalAmount,
                originalDate,
                finalAmount,
                finalDate
            );
        }

        if (!deferSave)
        {
            await userDataContext.SaveChangesAsync();
        }

        void UpdateBalancesForEditedTransaction(
            Transaction transaction,
            decimal originalAmount,
            DateOnly originalDate,
            decimal finalAmount,
            DateOnly finalDate
        )
        {
            if (transaction.Account!.Source == AccountSource.Manual)
            {
                SubtractAmountFromBalances(transaction, originalAmount, originalDate);
                CreateBalanceForDateIfNotExists(transaction.Account, finalDate);
                AddAmountToBalances(transaction, finalAmount, finalDate);
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteTransactionsAsync(
        Guid userGuid,
        IEnumerable<Guid> transactionIds,
        bool deferSave = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var uniqueTransactionIds = transactionIds.Distinct().ToList();
        foreach (var transactionId in uniqueTransactionIds)
        {
            var transaction = GetTransactionByID(userData, transactionId);

            transaction.Deleted = nowProvider.UtcNow;
            transaction.Category = null;
            transaction.Subcategory = null;

            if (transaction.Account!.Source == AccountSource.Manual)
            {
                SubtractAmountFromBalances(transaction, transaction.Amount, transaction.Date);
            }
        }

        if (!deferSave)
        {
            await userDataContext.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task RestoreTransactionsAsync(
        Guid userGuid,
        IEnumerable<Guid> transactionIds,
        bool deferSave = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var uniqueTransactionIds = transactionIds.Distinct().ToList();
        foreach (var transactionId in uniqueTransactionIds)
        {
            var transaction = GetTransactionByID(userData, transactionId);

            transaction.Deleted = null;

            if (transaction.Account!.Source == AccountSource.Manual)
            {
                CreateBalanceForDateIfNotExists(transaction.Account, transaction.Date);
                AddAmountToBalances(transaction, transaction.Amount, transaction.Date);
            }
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
        var transaction = GetTransactionByID(userData, transactionSplitRequest.ID);

        if (Math.Abs(transaction.Amount) <= Math.Abs(transactionSplitRequest.Amount))
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionSplitInvalidAmountLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionSplitInvalidAmountError"]
            );
        }

        await UpdateTransactionsAsync(
            userGuid,
            new TransactionUpdateRequest[]
            {
                new()
                {
                    ID = transaction.ID,
                    Amount = transaction.Amount - transactionSplitRequest.Amount,
                },
            },
            true
        );

        await CreateTransactionAsync(
            userGuid,
            new TransactionCreateRequest
            {
                SyncID = transaction.SyncID,
                Amount = transactionSplitRequest.Amount,
                Date = transaction.Date,
                Category = transactionSplitRequest.Category,
                Subcategory = transactionSplitRequest.Subcategory,
                MerchantName = transaction.MerchantName,
                Source = transaction.Source,
                AccountID = transaction.AccountID,
            },
            true
        );

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ImportTransactionsAsync(Guid userGuid, ITransactionImportRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        foreach (var transaction in request.Transactions)
        {
            var accountId = request
                .AccountNameToIDMap.FirstOrDefault(a =>
                    a.AccountName.Equals(
                        transaction.Account,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                ?.AccountID;
            var account = GetAccountByID(userData, accountId ?? Guid.Empty);

            var newTransaction = new TransactionCreateRequest
            {
                SyncID = string.Empty,
                Amount = transaction.Amount ?? 0,
                Date = transaction.Date ?? nowProvider.Today,
                MerchantName = transaction.MerchantName,
                Source = TransactionSource.Manual,
                AccountID = account.ID,
            };

            var matchedCategory = allCategories.FirstOrDefault(c =>
                c.Value.Equals(transaction.Category, StringComparison.InvariantCultureIgnoreCase)
            );
            string coercedCategoryValue = matchedCategory?.Value ?? string.Empty;

            (newTransaction.Category, newTransaction.Subcategory) =
                TransactionCategoriesHelpers.GetFullCategory(coercedCategoryValue, allCategories);

            await CreateTransactionAsync(userData, newTransaction, true);
        }

        await userDataContext.SaveChangesAsync();
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

    private static void SubtractAmountFromBalances(
        Transaction transaction,
        decimal amount,
        DateOnly date
    )
    {
        var balancesAfterDate = transaction.Account!.Balances.Where(b => b.Date >= date);
        balancesAfterDate.ToList().ForEach(balance => balance.Amount -= amount);
    }

    private static void AddAmountToBalances(Transaction transaction, decimal amount, DateOnly date)
    {
        var balancesAfterDate = transaction.Account!.Balances.Where(b => b.Date >= date);
        balancesAfterDate.ToList().ForEach(balance => balance.Amount += amount);
    }

    private void CreateBalanceForDateIfNotExists(Account account, DateOnly date)
    {
        var existingBalance = account.Balances.FirstOrDefault(b => b.Date == date);
        if (existingBalance == null)
        {
            var precedingBalance = account
                .Balances.Where(b => b.Date < date)
                .OrderByDescending(b => b.Date)
                .FirstOrDefault();

            var newBalance = new Balance
            {
                Amount = precedingBalance?.Amount ?? 0,
                Date = date,
                AccountID = account.ID,
            };

            userDataContext.Balances.Add(newBalance);
        }
    }
}
