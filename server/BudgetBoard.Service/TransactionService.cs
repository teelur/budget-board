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
    IStringLocalizer<LogStrings> logLocalizer,
    ITagService tagService,
    IRecurringRuleService recurringRuleService
) : ITransactionService
{
    private const int DefaultLinkCandidateDateWindowDays = 3;
    private const string TransferCategory = "Transfer";

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
        await CreateTransactionAsync(userData, request, deferSave, null);
    }

    private async Task CreateTransactionAsync(
        ApplicationUser userData,
        ITransactionCreateRequest request,
        bool deferSave,
        Guid? transactionId
    )
    {
        var account = GetAccountByID(userData, request.AccountID);

        var newTransaction = new Transaction
        {
            ID = transactionId ?? Guid.NewGuid(),
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
        await recurringRuleService.MatchTransactionAsync(userData.Id, newTransaction);

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
        bool includeHiddenAccounts,
        bool includeHiddenCategory,
        bool includeDeleted
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);

        var transactions = userData.Accounts.SelectMany(t => t.Transactions);

        if (!includeDeleted)
        {
            transactions = transactions.Where(t => t.Deleted == null);
        }

        if (!includeHiddenAccounts)
        {
            transactions = transactions.Where(t => t.Account!.HideTransactions is false);
        }

        if (!includeHiddenCategory)
        {
            transactions = transactions.Where(t =>
                t.Category != TransactionCategoriesConstants.HideFromBudgetsCategory
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
        var removedTagIds = new HashSet<Guid>();
        foreach (var request in requests)
        {
            var transaction = GetTransactionByID(userData, request.ID);
            var originalAmount = transaction.Amount;
            var originalDate = transaction.Date;
            var finalAmount = request.Amount ?? originalAmount;
            var finalDate = request.Date ?? originalDate;

            var linkedTransaction = GetLinkedTransaction(transaction);
            if (
                linkedTransaction != null
                && request.Amount.HasValue
                && !AreOppositeAmounts(finalAmount, linkedTransaction.Amount)
            )
            {
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionLinkedAmountUpdateError"]
                );
            }

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
            if (request.Notes is not null)
            {
                transaction.Notes = request.Notes;
            }

            if (request.AddTags is not null || request.RemoveTags is not null)
            {
                removedTagIds.UnionWith(
                    await tagService.ApplyTagChangesAsync(
                        userData.Id,
                        transaction,
                        request.AddTags,
                        request.RemoveTags
                    )
                );
            }

            await recurringRuleService.MatchTransactionAsync(userData.Id, transaction);

            UpdateBalancesForEditedTransaction(
                transaction,
                originalAmount,
                originalDate,
                finalAmount,
                finalDate
            );
        }

        await tagService.DeleteOrphanedTagsAsync(userData.Id, removedTagIds);

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
        var removedTagIds = new HashSet<Guid>();

        var uniqueTransactionIds = transactionIds.Distinct().ToList();
        var linksToRemove = new HashSet<TransactionLink>();
        foreach (var transactionId in uniqueTransactionIds)
        {
            var transaction = GetTransactionByID(userData, transactionId);

            var linkedTransactionLink = GetTransactionLink(transaction);
            if (linkedTransactionLink != null)
            {
                linksToRemove.Add(linkedTransactionLink);
            }

            transaction.Deleted = nowProvider.UtcNow;
            transaction.Category = null;
            transaction.Subcategory = null;
            removedTagIds.UnionWith(await tagService.RemoveAllTagsAsync(transaction));

            if (transaction.Account!.Source == AccountSource.Manual)
            {
                SubtractAmountFromBalances(transaction, transaction.Amount, transaction.Date);
            }
        }

        userDataContext.TransactionLinks.RemoveRange(linksToRemove);

        await tagService.DeleteOrphanedTagsAsync(userData.Id, removedTagIds);

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

        if (GetTransactionLink(transaction) != null)
        {
            throw new BudgetBoardServiceException(responseLocalizer["TransactionLinkedSplitError"]);
        }

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
    public async Task<IReadOnlyList<ITransactionResponse>> ReadTransactionLinkCandidatesAsync(
        Guid userGuid,
        Guid transactionID,
        int dateWindowDays = DefaultLinkCandidateDateWindowDays
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var transaction = GetTransactionByID(userData, transactionID);

        if (transaction.Deleted != null || transaction.Account?.Deleted != null)
        {
            throw new BudgetBoardServiceException(responseLocalizer["TransactionLinkDeletedError"]);
        }

        var linkedTransactionIDs = await userDataContext
            .TransactionLinks.Select(link => link.SourceTransactionID)
            .Concat(userDataContext.TransactionLinks.Select(link => link.TargetTransactionID))
            .ToHashSetAsync();

        if (linkedTransactionIDs.Contains(transaction.ID))
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionAlreadyLinkedError"]
            );
        }

        var boundedDateWindow = Math.Clamp(dateWindowDays, 0, 365);
        return userData
            .Accounts.SelectMany(account => account.Transactions)
            .Where(candidate =>
                candidate.ID != transaction.ID
                && candidate.Deleted == null
                && candidate.Account?.Deleted == null
                && candidate.AccountID != transaction.AccountID
                && AreOppositeAmounts(transaction.Amount, candidate.Amount)
                && Math.Abs(candidate.Date.DayNumber - transaction.Date.DayNumber)
                    <= boundedDateWindow
                && !linkedTransactionIDs.Contains(candidate.ID)
            )
            .OrderBy(candidate => Math.Abs(candidate.Date.DayNumber - transaction.Date.DayNumber))
            .ThenBy(candidate => candidate.Account!.Name)
            .ThenBy(candidate => candidate.MerchantName)
            .ThenBy(candidate => candidate.ID)
            .Select(candidate => (ITransactionResponse)new TransactionResponse(candidate))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionResponse>> LinkTransactionsAsync(
        Guid userGuid,
        ITransactionLinkRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var sourceTransaction = GetTransactionByID(userData, request.TransactionID);
        var targetTransaction = GetTransactionByID(userData, request.LinkedTransactionID);

        if (sourceTransaction.ID == targetTransaction.ID)
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionLinkSameTransactionError"]
            );
        }

        if (
            sourceTransaction.Deleted != null
            || targetTransaction.Deleted != null
            || sourceTransaction.Account?.Deleted != null
            || targetTransaction.Account?.Deleted != null
        )
        {
            throw new BudgetBoardServiceException(responseLocalizer["TransactionLinkDeletedError"]);
        }

        if (sourceTransaction.AccountID == targetTransaction.AccountID)
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionLinkSameAccountError"]
            );
        }

        if (!AreOppositeAmounts(sourceTransaction.Amount, targetTransaction.Amount))
        {
            throw new BudgetBoardServiceException(responseLocalizer["TransactionLinkAmountsError"]);
        }

        if (
            await HasTransactionLinkAsync(sourceTransaction.ID)
            || await HasTransactionLinkAsync(targetTransaction.ID)
        )
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionAlreadyLinkedError"]
            );
        }

        sourceTransaction.Category = TransferCategory;
        sourceTransaction.Subcategory = NormalizeTransferSubcategory(
            userData,
            sourceTransaction.Subcategory
        );
        targetTransaction.Category = TransferCategory;
        targetTransaction.Subcategory = NormalizeTransferSubcategory(
            userData,
            targetTransaction.Subcategory
        );

        var linkDate =
            sourceTransaction.Date <= targetTransaction.Date
                ? sourceTransaction.Date
                : targetTransaction.Date;
        UpdateBalanceForDateChange(sourceTransaction, linkDate);
        UpdateBalanceForDateChange(targetTransaction, linkDate);
        sourceTransaction.Date = linkDate;
        targetTransaction.Date = linkDate;

        var link = new TransactionLink
        {
            SourceTransactionID = sourceTransaction.ID,
            SourceTransaction = sourceTransaction,
            TargetTransactionID = targetTransaction.ID,
            TargetTransaction = targetTransaction,
        };
        sourceTransaction.SourceTransactionLink = link;
        targetTransaction.TargetTransactionLink = link;
        userDataContext.TransactionLinks.Add(link);
        try
        {
            await userDataContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionAlreadyLinkedError"],
                exception
            );
        }

        return
        [
            new TransactionResponse(sourceTransaction),
            new TransactionResponse(targetTransaction),
        ];

        void UpdateBalanceForDateChange(Transaction transaction, DateOnly finalDate)
        {
            if (
                transaction.Date == finalDate
                || transaction.Account!.Source != AccountSource.Manual
            )
            {
                return;
            }

            SubtractAmountFromBalances(transaction, transaction.Amount, transaction.Date);
            CreateBalanceForDateIfNotExists(transaction.Account, finalDate);
            AddAmountToBalances(transaction, transaction.Amount, finalDate);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionResponse>> UnlinkTransactionAsync(
        Guid userGuid,
        Guid transactionID
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var transaction = GetTransactionByID(userData, transactionID);
        var link =
            await userDataContext
                .TransactionLinks.AsNoTracking()
                .Where(link =>
                    link.SourceTransactionID == transactionID
                    || link.TargetTransactionID == transactionID
                )
                .Select(link => new { link.SourceTransactionID, link.TargetTransactionID })
                .FirstOrDefaultAsync()
            ?? throw new BudgetBoardServiceException(
                responseLocalizer["TransactionNotLinkedError"]
            );
        var linkedTransaction = GetTransactionByID(
            userData,
            link.SourceTransactionID == transactionID
                ? link.TargetTransactionID
                : link.SourceTransactionID
        );

        if (userDataContext.Database.IsRelational())
        {
            await userDataContext
                .TransactionLinks.Where(transactionLink =>
                    transactionLink.SourceTransactionID == transactionID
                    || transactionLink.TargetTransactionID == transactionID
                )
                .ExecuteDeleteAsync();
        }
        else
        {
            var trackedLinks = await userDataContext
                .TransactionLinks.Where(transactionLink =>
                    transactionLink.SourceTransactionID == transactionID
                    || transactionLink.TargetTransactionID == transactionID
                )
                .ToListAsync();
            userDataContext.TransactionLinks.RemoveRange(trackedLinks);
            await userDataContext.SaveChangesAsync();
        }

        transaction.SourceTransactionLink = null;
        transaction.TargetTransactionLink = null;
        linkedTransaction.SourceTransactionLink = null;
        linkedTransaction.TargetTransactionLink = null;

        foreach (var entry in userDataContext.ChangeTracker.Entries<TransactionLink>())
        {
            entry.State = EntityState.Detached;
        }

        return [new TransactionResponse(transaction), new TransactionResponse(linkedTransaction)];
    }

    /// <inheritdoc />
    public async Task ImportTransactionsAsync(Guid userGuid, ITransactionImportRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var transactionIds = userData
            .Accounts.SelectMany(account => account.Transactions)
            .Select(transaction => transaction.ID)
            .ToHashSet();

        foreach (var transaction in request.Transactions)
        {
            if (transaction.ID is Guid transactionID && !transactionIds.Add(transactionID))
            {
                continue;
            }

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

            await CreateTransactionAsync(userData, newTransaction, true, transaction.ID);
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
                    .ThenInclude(t => t.TransactionTags)
                    .ThenInclude(transactionTag => transactionTag.Tag)
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Transactions)
                    .ThenInclude(t => t.SourceTransactionLink)
                    .ThenInclude(link => link!.TargetTransaction)
                    .ThenInclude(transaction => transaction!.Account)
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Transactions)
                    .ThenInclude(t => t.TargetTransactionLink)
                    .ThenInclude(link => link!.SourceTransaction)
                    .ThenInclude(transaction => transaction!.Account)
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

    private static TransactionLink? GetTransactionLink(Transaction transaction)
    {
        return transaction.SourceTransactionLink ?? transaction.TargetTransactionLink;
    }

    private static Transaction? GetLinkedTransaction(Transaction transaction)
    {
        return transaction.SourceTransactionLink?.TargetTransaction
            ?? transaction.TargetTransactionLink?.SourceTransaction;
    }

    private Task<bool> HasTransactionLinkAsync(Guid transactionID)
    {
        return userDataContext.TransactionLinks.AnyAsync(link =>
            link.SourceTransactionID == transactionID || link.TargetTransactionID == transactionID
        );
    }

    private static bool AreOppositeAmounts(decimal firstAmount, decimal secondAmount)
    {
        return firstAmount != 0 && firstAmount == -secondAmount;
    }

    private static string? NormalizeTransferSubcategory(
        ApplicationUser userData,
        string? subcategory
    )
    {
        if (string.IsNullOrWhiteSpace(subcategory))
        {
            return null;
        }

        var isValidTransferSubcategory =
            TransactionCategoriesConstants.DefaultTransactionCategories.Any(category =>
                category.Parent.Equals(TransferCategory, StringComparison.OrdinalIgnoreCase)
                && category.Value.Equals(subcategory, StringComparison.OrdinalIgnoreCase)
            )
            || userData.TransactionCategories.Any(category =>
                category.Parent.Equals(TransferCategory, StringComparison.OrdinalIgnoreCase)
                && category.Value.Equals(subcategory, StringComparison.OrdinalIgnoreCase)
            );
        return isValidTransferSubcategory ? subcategory : null;
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
