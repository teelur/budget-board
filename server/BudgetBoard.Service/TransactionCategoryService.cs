using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class TransactionCategoryService(
    ILogger<ITransactionCategoryService> logger,
    UserDataContext userDataContext
) : ITransactionCategoryService
{
    private readonly ILogger<ITransactionCategoryService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateTransactionCategoryAsync(Guid userGuid, ICategoryCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase()
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        IEnumerable<ICategory> allCategories;
        if (userData.UserSettings?.DisableBuiltInTransactionCategories == true)
        {
            allCategories = customCategories;
        }
        else
        {
            allCategories = TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
                customCategories
            );
        }

        if (
            allCategories.Any(c =>
                c.Value.Equals(request.Value, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            _logger.LogError("Attempt to create a duplicate transaction category.");
            throw new BudgetBoardServiceException("Transaction category already exists.");
        }

        if (string.IsNullOrEmpty(request.Value))
        {
            _logger.LogError("Attempt to create a transaction category without a value.");
            throw new BudgetBoardServiceException("Transaction category must have a name.");
        }

        if (request.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(
                "Attempt to create a transaction category with the same name as its parent category."
            );
            throw new BudgetBoardServiceException(
                "Transaction category cannot have the same name as its parent category."
            );
        }

        if (
            !string.IsNullOrEmpty(request.Parent)
            && !allCategories.Any(c =>
                c.Value.Equals(request.Parent, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            _logger.LogError(
                "Attempt to create a transaction category with a parent that does not exist."
            );
            throw new BudgetBoardServiceException("Parent category does not exist.");
        }

        var newCategory = new Category
        {
            Value = request.Value,
            Parent = request.Parent,
            UserID = userData.Id,
        };

        userData.TransactionCategories.Add(newCategory);
        await _userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ICategoryResponse>> ReadTransactionCategoriesAsync(
        Guid userGuid,
        Guid categoryGuid = default
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        if (categoryGuid != default)
        {
            var transactionCategory = userData.TransactionCategories.FirstOrDefault(t =>
                t.ID == categoryGuid
            );
            if (transactionCategory == null)
            {
                _logger.LogError("Attempt to access transaction category that does not exist.");
                throw new BudgetBoardServiceException(
                    "The transaction category you are trying to access does not exist."
                );
            }

            return [new CategoryResponse(transactionCategory)];
        }

        return userData.TransactionCategories.Select(c => new CategoryResponse(c));
    }

    public async Task UpdateTransactionCategoryAsync(
        Guid userGuid,
        ICategoryUpdateRequest updatedCategory
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t =>
            t.ID == updatedCategory.ID
        );
        if (transactionCategory == null)
        {
            _logger.LogError("Attempt to access transaction category that does not exist.");
            throw new BudgetBoardServiceException(
                "The transaction category you are trying to access does not exist."
            );
        }

        transactionCategory.Value = updatedCategory.Value;
        transactionCategory.Parent = updatedCategory.Parent;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteTransactionCategoryAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t => t.ID == guid);
        if (transactionCategory == null)
        {
            _logger.LogError("Attempt to delete transaction category that does not exist.");
            throw new BudgetBoardServiceException(
                "The transaction category you are trying to delete does not exist."
            );
        }

        // We want to preserve the category in the database if it is in use.
        var transactionsForUser = userData.Accounts.SelectMany(a => a.Transactions);
        if (
            transactionsForUser.Any(t =>
                (t.Category ?? string.Empty).Equals(
                    transactionCategory.Value,
                    StringComparison.OrdinalIgnoreCase
                )
                || (t.Subcategory ?? string.Empty).Equals(
                    transactionCategory.Value,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        )
        {
            _logger.LogError(
                "Attempt to delete transaction category that is in use by transaction(s)."
            );
            throw new BudgetBoardServiceException(
                "Category is in use by transaction(s) and cannot be deleted."
            );
        }
        else if (userData.Budgets.Any(b => b.Category == transactionCategory.Value))
        {
            _logger.LogError("Attempt to delete transaction category that is in use by budget(s).");
            throw new BudgetBoardServiceException(
                "Category is in use by budget(s) and cannot be deleted."
            );
        }
        else if (userData.TransactionCategories.Any(c => c.Parent == transactionCategory.Value))
        {
            _logger.LogError("Attempt to delete transaction category that is a parent category.");
            throw new BudgetBoardServiceException(
                "Transaction category has subcategories associated with it and cannot be deleted."
            );
        }
        else
        {
            userData.TransactionCategories.Remove(transactionCategory);
            await _userDataContext.SaveChangesAsync();
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.TransactionCategories)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Budgets)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while retrieving the user data: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while retrieving the user data."
            );
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
