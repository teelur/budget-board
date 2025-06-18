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

    public async Task CreateBudgetsAsync(
        Guid userGuid,
        IEnumerable<IBudgetCreateRequest> budgets,
        bool isCopy = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

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
                userData.TransactionCategories.Select(tc => new CategoryBase
                {
                    Value = tc.Value,
                    Parent = tc.Parent,
                })
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

    public async Task<IEnumerable<IBudgetResponse>> ReadBudgetsAsync(Guid userGuid, DateTime date)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budgets = userData.Budgets.Where(b =>
            b.Date.Month == date.Month && b.Date.Year == date.Year
        );

        return budgets.Select(b => new BudgetResponse(b));
    }

    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest updatedBudget)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = userData.Budgets.SingleOrDefault(b => b.ID == updatedBudget.ID);
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

        if (TransactionCategoriesHelpers.GetIsParentCategory(budget.Category, customCategories))
        {
            var childBudgets = userData
                .Budgets.Where(b =>
                    TransactionCategoriesHelpers
                        .GetParentCategory(b.Category, customCategories)
                        .Equals(budget.Category, StringComparison.CurrentCultureIgnoreCase)
                    && b.Date.Month == budget.Date.Month
                    && b.Date.Year == budget.Date.Year
                )
                .ToList();
            var childBudgetsLimitTotal = childBudgets.Sum(b => b.Limit);

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
                customCategories
            );

            if (!string.IsNullOrEmpty(parentCategory))
            {
                var parentBudget = userData.Budgets.SingleOrDefault(b =>
                    b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
                    && b.Date.Month == budget.Date.Month
                    && b.Date.Year == budget.Date.Year
                );

                var childBudgets = userData.Budgets.Where(b =>
                    TransactionCategoriesHelpers
                        .GetParentCategory(b.Category, customCategories)
                        .Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
                    && b.Date.Month == budget.Date.Month
                    && b.Date.Year == budget.Date.Year
                );
                var childBudgetsLimitTotal = childBudgets.Sum(b => b.Limit);

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

        if (
            TransactionCategoriesHelpers.GetIsParentCategory(
                budget.Category,
                userData.TransactionCategories.Select(tc => new CategoryBase
                {
                    Value = tc.Value,
                    Parent = tc.Parent,
                })
            )
        )
        {
            var childrenBudgets = userData
                .Budgets.Where(b =>
                    TransactionCategoriesHelpers
                        .GetParentCategory(
                            b.Category,
                            userData.TransactionCategories.Select(tc => new CategoryBase
                            {
                                Value = tc.Value,
                                Parent = tc.Parent,
                            })
                        )
                        .Equals(budget.Category, StringComparison.CurrentCultureIgnoreCase)
                    && b.Date.Month == budget.Date.Month
                    && b.Date.Year == budget.Date.Year
                )
                .ToList();
            foreach (var childBudget in childrenBudgets)
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

    private decimal GetBudgetChildrenLimit(
        string parentCategory,
        DateTime date,
        ApplicationUser userData
    )
    {
        var budgetsForMonth = userData
            .Budgets.Where(b => b.Date.Month == date.Month && b.Date.Year == date.Year)
            .ToList();

        var childrenBudgets = budgetsForMonth
            .Where(b =>
                TransactionCategoriesHelpers
                    .GetParentCategory(
                        b.Category,
                        _userDataContext.TransactionCategories.Select(tc => new CategoryBase
                        {
                            Value = tc.Value,
                            Parent = tc.Parent,
                        })
                    )
                    .Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
            )
            .ToList();

        return childrenBudgets.Sum(b => b.Limit);
    }
}
