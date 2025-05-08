using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class TransactionService(ILogger<ITransactionService> logger, UserDataContext userDataContext) : ITransactionService
{
    private readonly ILogger<ITransactionService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateTransactionAsync(Guid userGuid, ITransactionCreateRequest transaction)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == transaction.AccountID);
        if (account == null)
        {
            _logger.LogError("Attempt to add transaction to account that does not exist.");
            throw new BudgetBoardServiceException("The account you are trying to add a transaction to does not exist.");
        }

        var newTransaction = new Transaction
        {
            SyncID = transaction.SyncID,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Category = transaction.Category,
            Subcategory = transaction.Subcategory,
            MerchantName = transaction.MerchantName,
            Source = transaction.Source ?? TransactionSource.Manual.Value,
            AccountID = transaction.AccountID
        };

        account.Transactions.Add(newTransaction);

        // Manual accounts need to manually update the balance
        if (account.Source == AccountSource.Manual)
        {
            var currentBalance = account.Balances.Where(b => b.DateTime <= transaction.Date).OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0;

            // First, add the new balance for the new transaction.
            var newBalance = new Balance
            {
                Amount = transaction.Amount + currentBalance,
                DateTime = transaction.Date,
                AccountID = account.ID
            };

            account.Balances.Add(newBalance);

            // Then, update all following balances to include the new transaction.
            var balancesAfterNew = account.Balances.Where(b => b.DateTime > transaction.Date).ToList();
            foreach (var balance in balancesAfterNew)
            {
                balance.Amount += transaction.Amount;
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ITransactionResponse>> ReadTransactionsAsync(Guid userGuid, int? year, int? month, bool getHidden, Guid guid = default)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transactions = userData.Accounts
                .SelectMany(t => t.Transactions)
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
                _logger.LogError("Attempt to access transaction that does not exist.");
                throw new BudgetBoardServiceException("The transaction you are trying to access does not exist.");
            }

            return [new TransactionResponse(transaction)];
        }

        return transactions.Select(t => new TransactionResponse(t));
    }

    public async Task UpdateTransactionAsync(Guid userGuid, ITransactionUpdateRequest editedTransaction)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transaction = userData.Accounts
            .SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == editedTransaction.ID);
        if (transaction == null)
        {
            _logger.LogError("Attempt to edit transaction that does not exist.");
            throw new BudgetBoardServiceException("The transaction you are trying to edit does not exist.");
        }

        var amountDifference = editedTransaction.Amount - transaction.Amount;

        transaction.Amount = editedTransaction.Amount;
        transaction.Date = editedTransaction.Date;
        transaction.Category = editedTransaction.Category;
        transaction.Subcategory = editedTransaction.Subcategory;
        transaction.MerchantName = editedTransaction.MerchantName;

        if (transaction.Account?.Source == AccountSource.Manual)
        {
            // Update all following balances to include the edited transaction.
            var balancesAfterEdited = transaction.Account.Balances.Where(b => b.DateTime >= transaction.Date).ToList();
            foreach (var balance in balancesAfterEdited)
            {
                balance.Amount += amountDifference;
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteTransactionAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transaction = userData.Accounts
            .SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == guid);
        if (transaction == null)
        {
            _logger.LogError("Attempt to delete transaction that does not exist.");
            throw new BudgetBoardServiceException("The transaction you are trying to delete does not exist.");
        }

        transaction.Deleted = DateTime.Now.ToUniversalTime();

        var account = userData.Accounts.FirstOrDefault(a => a.ID == transaction.AccountID);
        if (account == null)
        {
            _logger.LogError("Transaction has no associated account.");
            throw new BudgetBoardServiceException("The transaction you are deleting has no associated account.");
        }

        // Manual accounts need to manually update the balance
        if (account.Source == AccountSource.Manual)
        {
            // Update all following balances to not include the deleted transaction.
            var balancesAfterDeleted = account.Balances.Where(b => b.DateTime >= transaction.Date).ToList();
            foreach (var balance in balancesAfterDeleted)
            {
                balance.Amount -= transaction.Amount;
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    public async Task RestoreTransactionAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transaction = userData.Accounts
            .SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == guid);
        if (transaction == null)
        {
            _logger.LogError("Attempt to restore transaction that does not exist.");
            throw new BudgetBoardServiceException("The transaction you are trying to restore does not exist.");
        }

        transaction.Deleted = null;
        await _userDataContext.SaveChangesAsync();
    }

    public async Task SplitTransactionAsync(Guid userGuid, ITransactionSplitRequest transactionSplitRequest)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transaction = userData.Accounts
            .SelectMany(t => t.Transactions)
            .FirstOrDefault(t => t.ID == transactionSplitRequest.ID);
        if (transaction == null)
        {
            _logger.LogError("Attempt to split transaction that does not exist.");
            throw new BudgetBoardServiceException("The transaction you are trying to split does not exist.");
        }

        if (Math.Abs(transaction.Amount) <= Math.Abs(transactionSplitRequest.Amount))
        {
            _logger.LogError("Attempt to split transaction with amount less than or equal to the split amount.");
            throw new BudgetBoardServiceException("The split amount must be less than the transaction amount.");
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
            AccountID = transaction.AccountID
        };

        transaction.Amount -= transactionSplitRequest.Amount;

        var account = userData.Accounts.FirstOrDefault(a => a.ID == transaction.AccountID);
        if (account == null)
        {
            _logger.LogError("Transaction has no associated account.");
            throw new BudgetBoardServiceException("The transaction you are splitting has no associated account.");
        }

        account.Transactions.Add(splitTransaction);
        await _userDataContext.SaveChangesAsync();
    }

    public async Task ImportTransactionsAsync(Guid userGuid, ITransactionImportRequest transactionImportRequest)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transactions = transactionImportRequest.Transactions;
        var accountNameToIDMap = transactionImportRequest.AccountNameToIDMap;
        foreach (var transaction in transactions)
        {
            var account = userData.Accounts.FirstOrDefault(a => a.ID == accountNameToIDMap.FirstOrDefault(a => a.AccountName == transaction.Account)?.AccountID);
            if (account == null)
            {
                _logger.LogError("Attempt to add transaction to account that does not exist.");
                throw new BudgetBoardServiceException("The account you are trying to add a transaction to does not exist.");
            }

            var newTransaction = new TransactionCreateRequest
            {
                SyncID = string.Empty,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Category = transaction.Category ?? string.Empty,
                Subcategory = string.Empty,
                MerchantName = transaction.Description,
                Source = TransactionSource.Manual.Value,
                AccountID = account.ID
            };

            await CreateTransactionAsync(userGuid, newTransaction);
        }
    }

    private async Task<IApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext.ApplicationUsers
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Balances)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while retrieving the user data: {ExceptionMessage}", ex.Message);
            throw new BudgetBoardServiceException("An error occurred while retrieving the user data.");
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
