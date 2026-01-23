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
    private readonly ILogger<ITransactionService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly INowProvider _nowProvider = nowProvider;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateTransactionAsync(
        ApplicationUser userData,
        ITransactionCreateRequest request,
        IEnumerable<ICategory>? allCategories = null,
        AutomaticTransactionCategorizer? autoCategorizer = null
    )
    {
        var account = userData.Accounts.FirstOrDefault(a => a.ID == request.AccountID);
        if (account == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionCreateAccountNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionCreateAccountNotFoundError"]
            );
        }

        var newTransaction = new Transaction
        {
            SyncID = request.SyncID,
            Amount = request.Amount,
            Date = request.Date.ToUniversalTime(),
            Category = request.Category,
            Subcategory = request.Subcategory,
            MerchantName = request.MerchantName,
            Source = request.Source ?? TransactionSource.Manual.Value,
            AccountID = request.AccountID,
        };

        // Auto categorize
        if (
            autoCategorizer is not null &&
            allCategories is not null &&
            newTransaction.MerchantName is not null &&
            newTransaction.MerchantName != string.Empty
            )
        {
            var matchedCategory = autoCategorizer.Predict(newTransaction);
            (newTransaction.Category, newTransaction.Subcategory) =
                TransactionCategoriesHelpers.GetFullCategory(matchedCategory, allCategories);
        }

        _userDataContext.Transactions.Add(newTransaction);

        // Manual accounts need to manually update the balance
        if (account.Source == AccountSource.Manual)
        {
            UpdateBalancesForNewTransaction(account, request);
        }

        await _userDataContext.SaveChangesAsync();
    }

    public async Task CreateTransactionAsync(
        Guid userGuid,
        ITransactionCreateRequest request,
        IEnumerable<ICategory>? allCategories = null,
        AutomaticTransactionCategorizer? autoCategorizer = null
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        await CreateTransactionAsync(userData, request, allCategories, autoCategorizer);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionResponse>> ReadTransactionsAsync(
        Guid userGuid,
        int? year,
        int? month,
        bool getHidden,
        Guid guid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transactions = userData
            .Accounts.SelectMany(t => t.Transactions)
            .Where(t => getHidden || !(t.Account?.HideTransactions ?? false));

        if (year != null)
        {
            transactions = transactions.Where(t => t.Date.Year == year);
        }
        if (month != null)
        {
            transactions = transactions.Where(t => t.Date.Month == month);
        }

        if (guid != default)
        {
            var transaction = transactions.FirstOrDefault(t => t.ID == guid);
            if (transaction == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["TransactionNotFoundLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["TransactionNotFoundError"]
                );
            }

            return [new TransactionResponse(transaction)];
        }

        return transactions.Select(t => new TransactionResponse(t)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateTransactionAsync(
        Guid userGuid,
        ITransactionUpdateRequest editedTransaction
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == editedTransaction.ID);
        if (transaction == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionUpdateNotFoundError"]
            );
        }

        var amountDifference = editedTransaction.Amount - transaction.Amount;

        transaction.Amount = editedTransaction.Amount;
        transaction.Date = editedTransaction.Date.ToUniversalTime();
        transaction.Category = editedTransaction.Category;
        transaction.Subcategory = editedTransaction.Subcategory;
        transaction.MerchantName = editedTransaction.MerchantName;

        if (transaction.Account?.Source == AccountSource.Manual)
        {
            // Update all following balances to include the edited transaction.
            var balancesAfterEdited = transaction
                .Account.Balances.Where(b => b.DateTime >= transaction.Date)
                .ToList();
            foreach (var balance in balancesAfterEdited)
            {
                balance.Amount += amountDifference;
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteTransactionAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == guid);
        if (transaction == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionDeleteNotFoundError"]
            );
        }

        transaction.Deleted = _nowProvider.UtcNow;

        Account account = transaction.Account!;
        // Manual accounts need to manually update the balance
        if (account.Source == AccountSource.Manual)
        {
            // Update all following balances to not include the deleted transaction.
            var balancesAfterDeleted = account
                .Balances.Where(b => b.DateTime >= transaction.Date)
                .ToList();
            foreach (var balance in balancesAfterDeleted)
            {
                balance.Amount -= transaction.Amount;
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RestoreTransactionAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == guid);
        if (transaction == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionRestoreNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionRestoreNotFoundError"]
            );
        }

        transaction.Deleted = null;
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SplitTransactionAsync(
        Guid userGuid,
        ITransactionSplitRequest transactionSplitRequest
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transaction = userData
            .Accounts.SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == transactionSplitRequest.ID);
        if (transaction == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionSplitNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionSplitNotFoundError"]
            );
        }

        if (Math.Abs(transaction.Amount) <= Math.Abs(transactionSplitRequest.Amount))
        {
            _logger.LogError("{LogMessage}", _logLocalizer["TransactionSplitInvalidAmountLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["TransactionSplitInvalidAmountError"]
            );
        }

        var splitTransaction = new Transaction
        {
            SyncID = transaction.SyncID,
            Amount = transactionSplitRequest.Amount,
            Date = transaction.Date.ToUniversalTime(),
            Category = transactionSplitRequest.Category,
            Subcategory = transactionSplitRequest.Subcategory,
            MerchantName = transaction.MerchantName,
            Source = transaction.Source ?? TransactionSource.Manual.Value,
            AccountID = transaction.AccountID,
        };

        transaction.Amount -= transactionSplitRequest.Amount;

        _userDataContext.Transactions.Add(splitTransaction);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ImportTransactionsAsync(
        Guid userGuid,
        ITransactionImportRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transactions = request.Transactions;
        var accountNameToIDMap = request.AccountNameToIDMap;

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var autoCategorizer = await AutomaticTransactionCategorizer.CreateAutoCategorizerAsync(userDataContext, userData);

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
                _logger.LogError(
                    "{LogMessage}",
                    _logLocalizer["TransactionImportAccountNotFoundLog"]
                );
                throw new BudgetBoardServiceException(
                    _responseLocalizer["TransactionImportAccountNotFoundError"]
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
                Date = transaction.Date ?? _nowProvider.UtcNow,
                MerchantName = transaction.MerchantName,
                Source = TransactionSource.Manual.Value,
                AccountID = account.ID,
            };

            (newTransaction.Category, newTransaction.Subcategory) =
                TransactionCategoriesHelpers.GetFullCategory(matchedCategory, allCategories);

            await CreateTransactionAsync(userData, newTransaction, allCategories, autoCategorizer);
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .Include(u => u.UserSettings)
                .Include(u => u.TransactionCategories)
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

    private void UpdateBalancesForNewTransaction(
        Account account,
        ITransactionCreateRequest transaction
    )
    {
        var currentBalance = account
            .Balances.Where(b => b.DateTime.Date <= transaction.Date.ToUniversalTime().Date)
            .OrderByDescending(b => b.DateTime)
            .FirstOrDefault();

        // First, add the new balance for the new transaction if no balance exists for that date.
        if (
            currentBalance == null
            || currentBalance.DateTime.Date != transaction.Date.ToUniversalTime().Date
        )
        {
            var newBalance = new Balance
            {
                Amount = currentBalance?.Amount ?? 0,
                DateTime = transaction.Date.ToUniversalTime().Date,
                AccountID = account.ID,
            };

            _userDataContext.Balances.Add(newBalance);
        }

        // Then, update all following balances to include the new transaction.
        var balancesAfterNew = account
            .Balances.Where(b => b.DateTime.Date >= transaction.Date.ToUniversalTime().Date)
            .ToList();
        foreach (var balance in balancesAfterNew)
        {
            balance.Amount += transaction.Amount;
        }
    }
}
