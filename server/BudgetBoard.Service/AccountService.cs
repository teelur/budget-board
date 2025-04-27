using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class AccountService(ILogger<IAccountService> logger, UserDataContext userDataContext) : IAccountService
{
    private readonly ILogger<IAccountService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateAccountAsync(Guid userGuid, IAccountCreateRequest account)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (userData.Accounts.Any(a => a.Name.Equals(account.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogError("Attempted to create an account with a duplicate name.");
            throw new BudgetBoardServiceException("An account with this name already exists!");
        }

        if (!userData.Institutions.Any(i => i.ID == account.InstitutionID))
        {
            _logger.LogError("Attempted to create an account with an invalid institution ID.");
            throw new BudgetBoardServiceException("Invalid Institution ID.");
        }

        var institution = userData.Institutions.Single(i => i.ID == account.InstitutionID);

        if (institution.Deleted.HasValue)
        {
            // If the institution is deleted, we need to restore it.
            institution.Deleted = null;
        }

        var newAccount = new Account
        {
            SyncID = account.SyncID,
            Name = account.Name,
            InstitutionID = account.InstitutionID,
            Type = account.Type,
            Subtype = account.Subtype,
            HideTransactions = account.HideTransactions,
            HideAccount = account.HideAccount,
            Source = account.Source ?? AccountSource.Manual,
            UserID = userData.Id
        };

        userData.Accounts.Add(newAccount);
        await _userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<IAccountResponse>> ReadAccountsAsync(Guid userGuid, Guid accountGuid = default)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        if (accountGuid != default)
        {
            var account = userData.Accounts.FirstOrDefault(a => a.ID == accountGuid);
            if (account == null)
            {
                _logger.LogError("Attempt to access account that does not exist.");
                throw new BudgetBoardServiceException("The account you are trying to access does not exist.");
            }

            return [new AccountResponse(account)];
        }

        return userData.Accounts.Select(a => new AccountResponse(a));
    }

    public async Task UpdateAccountAsync(Guid userGuid, IAccountUpdateRequest editedAccount)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == editedAccount.ID);
        if (account == null)
        {
            _logger.LogError("Attempt to edit account that does not exist.");
            throw new BudgetBoardServiceException("The account you are trying to edit does not exist.");
        }

        account.Name = editedAccount.Name;
        account.Type = editedAccount.Type;
        account.Subtype = editedAccount.Subtype;
        account.HideTransactions = editedAccount.HideTransactions;
        account.HideAccount = editedAccount.HideAccount;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(Guid userGuid, Guid guid, bool deleteTransactions = false)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == guid);
        if (account == null)
        {
            _logger.LogError("Attempt to delete account that does not exist.");
            throw new BudgetBoardServiceException("The account you are trying to delete does not exist.");
        }

        account.Deleted = DateTime.Now.ToUniversalTime();

        if (deleteTransactions)
        {
            foreach (var transaction in account.Transactions)
            {
                transaction.Deleted = DateTime.Now.ToUniversalTime();
            }
        }

        if (account.Institution?.Accounts.All(a => a.Deleted != null) ?? false)
        {
            account.Institution.Deleted = DateTime.Now.ToUniversalTime();
            account.Institution.Index = 0;
        }

        await _userDataContext.SaveChangesAsync();
    }

    public async Task RestoreAccountAsync(Guid userGuid, Guid guid, bool restoreTransactions = false)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var account = userData.Accounts.FirstOrDefault(a => a.ID == guid);
        if (account == null)
        {
            _logger.LogError("Attempt to restore account that does not exist.");
            throw new BudgetBoardServiceException("The account you are trying to restore does not exist.");
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

    public async Task OrderAccountsAsync(Guid userGuid, IEnumerable<IAccountIndexRequest> orderedAccounts)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        foreach (var orderedAccount in orderedAccounts)
        {
            var account = userData.Accounts.FirstOrDefault(a => a.ID == orderedAccount.ID);
            if (account == null)
            {
                _logger.LogError("Attempt to set index for account that does not exist.");
                throw new BudgetBoardServiceException("The account you are trying to set the index for does not exist.");
            }

            account.Index = orderedAccount.Index;
        }

        await _userDataContext.SaveChangesAsync();
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
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Institution)
                .Include(u => u.Institutions)
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
