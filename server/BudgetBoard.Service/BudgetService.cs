using System.Diagnostics;
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class BudgetService(ILogger<IBudgetService> logger, UserDataContext userDataContext)
    : IBudgetService
{
    private readonly ILogger<IBudgetService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    /// <inheritdoc />
    public async Task CreateBudgetsAsync(
        Guid userGuid,
        IEnumerable<IBudgetCreateRequest> budgets,
        bool isCopy = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories =
            userData.UserSettings?.DisableBuiltInTransactionCategories == true
                ? customCategories
                : TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
                    customCategories
                );

        int newBudgetsCount = 0;

        foreach (var budget in budgets)
        {
            // Ignore duplicate categories in a given month
            if (
                userData.Budgets.Any(
                    (b) =>
                        b.Date.Month == budget.Date.Month
                        && b.Date.Year == budget.Date.Year
                        && b.Category.Equals(
                            budget.Category,
                            StringComparison.CurrentCultureIgnoreCase
                        )
                )
            )
            {
                continue;
            }

            Budget newBudget = new()
            {
                Date = budget.Date,
                Category = budget.Category,
                Limit = budget.Limit,
                UserID = userData.Id,
            };
            userData.Budgets.Add(newBudget);
            newBudgetsCount++;

            var parentCategory = TransactionCategoriesHelpers.GetParentCategory(
                budget.Category,
                allCategories
            );

            // Copy operations will have all of the data we want to add in the data set,
            // so don't auto-generate parent.
            if (!string.IsNullOrEmpty(parentCategory) && !isCopy)
            {
                if (
                    !userData.Budgets.Any(
                        (b) =>
                            b.Category.Equals(
                                parentCategory,
                                StringComparison.CurrentCultureIgnoreCase
                            )
                            && b.Date.Month == budget.Date.Month
                            && b.Date.Year == budget.Date.Year
                    )
                )
                {
                    var newParentBudget = new Budget
                    {
                        Date = budget.Date,
                        Category = parentCategory,
                        Limit = GetBudgetChildrenLimit(parentCategory, budget.Date, userData),
                        UserID = userData.Id,
                    };
                    userData.Budgets.Add(newParentBudget);
                    newBudgetsCount++;
                }
                else
                {
                    var parentBudget = userData.Budgets.SingleOrDefault(b =>
                        b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
                        && b.Date.Month == budget.Date.Month
                        && b.Date.Year == budget.Date.Year
                    );

                    // This should not happen, since we check in the if statement above
                    if (parentBudget == null)
                    {
                        Debug.Fail(parentCategory + " budget not found.");
                        _logger.LogError(
                            "Parent budget not found for category {Category}.",
                            parentCategory
                        );

                        continue;
                    }

                    // We need to update the parent budget limit with our new child budget limit
                    if (parentBudget.Limit < parentBudget.Limit + budget.Limit)
                    {
                        parentBudget.Limit += budget.Limit;
                    }
                }
            }
        }

        await _userDataContext.SaveChangesAsync();

        if (newBudgetsCount == 0)
        {
            _logger.LogError("Attempt to create duplicate budgets.");
            throw new BudgetBoardServiceException(
                "The budget(s) you are trying to create already exist."
            );
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IBudgetResponse>> ReadBudgetsAsync(
        Guid userGuid,
        DateTime monthDate
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budgets = userData.Budgets.Where(b =>
            b.Date.Month == monthDate.Month && b.Date.Year == monthDate.Year
        );

        return budgets.Select(b => new BudgetResponse(b)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest updatedBudget)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = userData.Budgets.FirstOrDefault(b => b.ID == updatedBudget.ID);
        if (budget == null)
        {
            _logger.LogError("Attempt to update budget that does not exist.");
            throw new BudgetBoardServiceException(
                "The budget you are trying to update does not exist."
            );
        }

        budget.Limit = updatedBudget.Limit;

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories =
            userData.UserSettings?.DisableBuiltInTransactionCategories == true
                ? customCategories
                : TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
                    customCategories
                );

        if (TransactionCategoriesHelpers.GetIsParentCategory(budget.Category, allCategories))
        {
            var childBudgetsLimitTotal = GetBudgetChildrenLimit(
                budget.Category,
                budget.Date,
                userData
            );

            if (childBudgetsLimitTotal > budget.Limit)
            {
                _logger.LogError(
                    "Attempt to update parent budget to a value less than the sum of its children."
                );
                throw new BudgetBoardServiceException(
                    "The parent budget cannot be less than the sum of its children."
                );
            }
        }
        else
        {
            var parentCategory = TransactionCategoriesHelpers.GetParentCategory(
                budget.Category,
                allCategories
            );

            if (!string.IsNullOrEmpty(parentCategory))
            {
                var parentBudget = userData.Budgets.SingleOrDefault(b =>
                    b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
                    && b.Date.Month == budget.Date.Month
                    && b.Date.Year == budget.Date.Year
                );

                var childBudgetsLimitTotal = GetBudgetChildrenLimit(
                    parentCategory,
                    budget.Date,
                    userData
                );

                if (parentBudget != null)
                {
                    if (childBudgetsLimitTotal > parentBudget.Limit)
                    {
                        parentBudget.Limit = childBudgetsLimitTotal;
                    }
                }
                else
                {
                    // Any new budgets shouldn't run into this case, but for pre v2.2.0 budgets,
                    // we should create a parent budget if it doesn't exist
                    var newParentBudget = new Budget
                    {
                        Date = budget.Date,
                        Category = parentCategory,
                        Limit = childBudgetsLimitTotal,
                        UserID = userData.Id,
                    };
                    userData.Budgets.Add(newParentBudget);
                }
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = userData.Budgets.SingleOrDefault(b => b.ID == budgetGuid);
        if (budget == null)
        {
            _logger.LogError("Attempt to delete budget that does not exist.");
            throw new BudgetBoardServiceException(
                "The budget you are trying to delete does not exist."
            );
        }

        _userDataContext.Budgets.Remove(budget);

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories =
            userData.UserSettings?.DisableBuiltInTransactionCategories == true
                ? customCategories
                : TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
                    customCategories
                );

        if (TransactionCategoriesHelpers.GetIsParentCategory(budget.Category, allCategories))
        {
            var childBudgetsForMonth = GetChildBudgetsForMonth(
                userData,
                budget.Category,
                budget.Date
            );

            foreach (var childBudget in childBudgetsForMonth)
            {
                _userDataContext.Budgets.Remove(childBudget);
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.Budgets)
                .Include(u => u.TransactionCategories)
                .Include(u => u.UserSettings)
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

    private List<Budget> GetChildBudgetsForMonth(
        ApplicationUser? userData,
        string parentCategory,
        DateTime monthDate
    )
    {
        if (userData == null)
        {
            _logger.LogError("User data is null.");
            return [];
        }

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories =
            userData.UserSettings?.DisableBuiltInTransactionCategories == true
                ? customCategories
                : TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
                    customCategories
                );

        var childBudgets = userData.Budgets.Where(b =>
            !TransactionCategoriesHelpers.GetIsParentCategory(b.Category, allCategories)
            && TransactionCategoriesHelpers
                .GetParentCategory(b.Category, allCategories)
                .Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
        );

        var childBudgetsForMonth = childBudgets
            .Where(b => b.Date.Month == monthDate.Month && b.Date.Year == monthDate.Year)
            .ToList();

        return childBudgetsForMonth ?? [];
    }

    private decimal GetBudgetChildrenLimit(
        string parentCategory,
        DateTime date,
        ApplicationUser userData
    ) => GetChildBudgetsForMonth(userData, parentCategory, date).Sum(b => b.Limit);
}
