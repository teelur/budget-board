using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class BudgetService(ILogger<IBudgetService> logger, UserDataContext userDataContext) : IBudgetService
{
    private readonly ILogger<IBudgetService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateBudgetsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> budgets)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        foreach (var budget in budgets)
        {
            // Do not allow duplicate categories in a given month
            if (userData.Budgets.Any((b) =>
            b.Date.Month == budget.Date.Month
            && b.Date.Year == budget.Date.Year
            && b.Category.Equals(budget.Category, StringComparison.CurrentCultureIgnoreCase)))
            {
                _logger.LogError("Attempt to create duplicate budget category.");
                throw new BudgetBoardServiceException("Budget category already exists for this month!");
            }

            Budget newBudget = new()
            {
                Date = budget.Date,
                Category = budget.Category,
                Limit = budget.Limit,
                UserID = userData.Id
            };
            userData.Budgets.Add(newBudget);

            var parentCategory =
                TransactionCategoriesHelpers.GetParentCategory(
                    budget.Category,
                    userData.TransactionCategories.Select(
                        tc => new CategoryBase
                        {
                            Value = tc.Value,
                            Parent = tc.Parent
                        }));
            if (!string.IsNullOrEmpty(parentCategory)
                )
            {
                if (!userData.Budgets.Any((b) =>
                    b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase) &&
                    b.Date.Month == budget.Date.Month &&
                    b.Date.Year == budget.Date.Year))
                {

                    var newParentBudget = new Budget
                    {
                        Date = budget.Date,
                        Category = parentCategory,
                        Limit = GetBudgetChildrenLimit(parentCategory, budget.Date, userData),
                        UserID = userData.Id
                    };
                    userData.Budgets.Add(newParentBudget);
                }
                else
                {
                    var parentBudget = userData.Budgets.SingleOrDefault(b =>
                        b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase) &&
                        b.Date.Month == budget.Date.Month &&
                        b.Date.Year == budget.Date.Year);
                    if (parentBudget != null && parentBudget.Limit < parentBudget.Limit + budget.Limit)
                    {
                        parentBudget.Limit += budget.Limit;
                    }
                }
            }
        }

        await _userDataContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<IBudgetResponse>> ReadBudgetsAsync(Guid userGuid, DateTime date)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budgets = userData.Budgets
        .Where(b => b.Date.Month == date.Month && b.Date.Year == date.Year);

        return budgets.Select(b => new BudgetResponse(b));
    }

    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest updatedBudget)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = userData.Budgets.SingleOrDefault(b => b.ID == updatedBudget.ID);
        if (budget == null)
        {
            _logger.LogError("Attempt to update budget that does not exist.");
            throw new BudgetBoardServiceException("The budget you are trying to update does not exist.");
        }

        // TODO: This logic is fucked. Figure it out.

        var oldLimit = budget.Limit;

        budget.Limit = updatedBudget.Limit;

        var parentCategory =
                TransactionCategoriesHelpers.GetParentCategory(
                    budget.Category,
                    userData.TransactionCategories.Select(
                        tc => new CategoryBase
                        {
                            Value = tc.Value,
                            Parent = tc.Parent
                        }));
        if (!string.IsNullOrEmpty(parentCategory))
        {
            var parentBudget = userData.Budgets.SingleOrDefault(b =>
                b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase));
            if (parentBudget != null)
            {
                if (parentBudget.Limit < parentBudget.Limit + updatedBudget.Limit - oldLimit)
                {
                    parentBudget.Limit += updatedBudget.Limit - oldLimit;
                }
                else
                {
                    parentBudget.Limit -= updatedBudget.Limit;
                }
            }
            else
            {
                var childrenLimitTotal = userData.Budgets
                    .Where(b =>
                        TransactionCategoriesHelpers.GetParentCategory(
                            b.Category, _userDataContext.TransactionCategories
                                .Select(tc => new CategoryBase
                                {
                                    Value = tc.Value,
                                    Parent = tc.Parent
                                }))
                        .Equals(budget.Category, StringComparison.CurrentCultureIgnoreCase))
                    .Sum(b => b.Limit);
                var newParentBudget = new Budget
                {
                    Date = budget.Date,
                    Category = parentCategory,
                    Limit = childrenLimitTotal - oldLimit + updatedBudget.Limit,
                    UserID = userData.Id
                };
                userData.Budgets.Add(newParentBudget);
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
            throw new BudgetBoardServiceException("The budget you are trying to delete does not exist.");
        }

        _userDataContext.Budgets.Remove(budget);

        if (TransactionCategoriesHelpers.GetIsParentCategory(
            budget.Category,
            userData.TransactionCategories.Select(tc => new CategoryBase
            {
                Value = tc.Value,
                Parent = tc.Parent
            })))
        {
            var childrenBudgets = userData.Budgets
               .Where(b => TransactionCategoriesHelpers.GetParentCategory(
                   b.Category, _userDataContext.TransactionCategories
                       .Select(tc => new CategoryBase
                       {
                           Value = tc.Value,
                           Parent = tc.Parent
                       }))
                   .Equals(budget.Category, StringComparison.CurrentCultureIgnoreCase))
               .ToList();
            var childrenBudgetsForMonth = childrenBudgets
                .Where(b => b.Date.Month == budget.Date.Month && b.Date.Year == budget.Date.Year)
                .ToList();
            foreach (var childBudget in childrenBudgetsForMonth)
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
            users = await _userDataContext.ApplicationUsers
                .Include(u => u.Budgets)
                .Include(u => u.TransactionCategories)
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

    private decimal GetBudgetChildrenLimit(string parentCategory, DateTime date, ApplicationUser userData)
    {
        var budgetsForMonth = userData.Budgets
            .Where(b => b.Date.Month == date.Month && b.Date.Year == date.Year)
            .ToList();

        var childrenBudgets = budgetsForMonth
            .Where(b => TransactionCategoriesHelpers.GetParentCategory(
                b.Category, _userDataContext.TransactionCategories
                    .Select(tc => new CategoryBase
                    {
                        Value = tc.Value,
                        Parent = tc.Parent
                    }))
            .Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        return childrenBudgets.Sum(b => b.Limit);
    }
}
