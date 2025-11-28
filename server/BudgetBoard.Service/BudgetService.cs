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

public class BudgetService(
    ILogger<IBudgetService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IBudgetService
{
    private readonly ILogger<IBudgetService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateBudgetsWithParentsAsync(
        Guid userGuid,
        IEnumerable<IBudgetCreateRequest> requests
    )
    {
        ApplicationUser userData = await GetCurrentUserAsync(userGuid.ToString());

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase()
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            customCategories,
            userData.UserSettings?.DisableBuiltInTransactionCategories ?? false
        );

        int newBudgetsCount = 0;
        foreach (var request in requests)
        {
            if (TryAddBudget(userData, request, out var newBudget))
            {
                newBudgetsCount++;

                if (TryAddParentBudget(userData, request, newBudget!, allCategories))
                {
                    newBudgetsCount++;
                }
            }
        }
        await _userDataContext.SaveChangesAsync();
        _logger.LogInformation(
            "{LogMessage}",
            _logLocalizer["BudgetCreateCompletedLog", newBudgetsCount]
        );
    }

    /// <inheritdoc />
    public async Task CreateBudgetsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> requests)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        int newBudgetsCount = 0;
        foreach (var request in requests)
        {
            if (TryAddBudget(userData, request, out var newBudget))
            {
                newBudgetsCount++;
            }
        }

        await _userDataContext.SaveChangesAsync();
        _logger.LogInformation(
            "{LogMessage}",
            _logLocalizer["BudgetCopyCompletedLog", newBudgetsCount]
        );
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
    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var budget = userData.Budgets.FirstOrDefault(b => b.ID == request.ID);
        if (budget == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["BudgetUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["BudgetUpdateNotFoundError"]);
        }

        budget.Limit = request.Limit;

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            customCategories,
            userData.UserSettings?.DisableBuiltInTransactionCategories ?? false
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
                _logger.LogError("{LogMessage}", _logLocalizer["BudgetUpdateParentLimitErrorLog"]);
                throw new BudgetBoardServiceException(
                    _responseLocalizer["BudgetUpdateParentLimitError"]
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
                    _userDataContext.Budgets.Add(newParentBudget);
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
            _logger.LogError("{LogMessage}", _logLocalizer["BudgetDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["BudgetDeleteNotFoundError"]);
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

    private static List<Budget> GetChildBudgetsForMonth(
        ApplicationUser userData,
        string parentCategory,
        DateTime monthDate
    )
    {
        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            customCategories,
            userData.UserSettings?.DisableBuiltInTransactionCategories ?? false
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

    private static decimal GetBudgetChildrenLimit(
        string parentCategory,
        DateTime date,
        ApplicationUser userData
    ) => GetChildBudgetsForMonth(userData, parentCategory, date).Sum(b => b.Limit);

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.Budgets)
                .Include(u => u.TransactionCategories)
                .Include(u => u.UserSettings)
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

    private bool TryAddBudget(
        ApplicationUser userData,
        IBudgetCreateRequest request,
        out Budget? newBudget
    )
    {
        var budgetForCategoryAlreadyExists = userData.Budgets.Any(b =>
            b.Date.Month == request.Date.Month
            && b.Date.Year == request.Date.Year
            && b.Category.Equals(request.Category, StringComparison.CurrentCultureIgnoreCase)
        );

        if (budgetForCategoryAlreadyExists)
        {
            _logger.LogWarning(
                "{LogMessage}",
                _logLocalizer[
                    "BudgetCreateDuplicateLog",
                    request.Category,
                    request.Date.ToString("yyyy-MM")
                ]
            );

            newBudget = null;
            return false;
        }

        newBudget = new Budget
        {
            Date = request.Date,
            Category = request.Category,
            Limit = request.Limit,
            UserID = userData.Id,
        };

        _userDataContext.Budgets.Add(newBudget);
        return true;
    }

    private bool TryAddParentBudget(
        ApplicationUser userData,
        IBudgetCreateRequest childRequest,
        Budget childBudget,
        IEnumerable<ICategory> allCategories
    )
    {
        var parentCategory = TransactionCategoriesHelpers.GetParentCategory(
            childRequest.Category,
            allCategories
        );

        if (
            string.IsNullOrEmpty(parentCategory)
            || parentCategory.Equals(
                childRequest.Category,
                StringComparison.CurrentCultureIgnoreCase
            )
        )
        {
            return false;
        }

        var parentBudgetRequest = new BudgetCreateRequest
        {
            Date = childRequest.Date,
            Category = parentCategory,
            Limit = GetBudgetChildrenLimit(parentCategory, childRequest.Date, userData),
        };

        if (!TryAddBudget(userData, parentBudgetRequest, out var newParentBudget))
        {
            UpdateParentBudgetLimit(parentCategory, childBudget, userData);
            return false;
        }

        _userDataContext.Budgets.Add(newParentBudget!);
        return true;
    }

    private void UpdateParentBudgetLimit(
        string parentCategory,
        Budget childBudget,
        ApplicationUser userData
    )
    {
        var parentBudget = userData.Budgets.SingleOrDefault(b =>
            b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
            && b.Date.Month == childBudget.Date.Month
            && b.Date.Year == childBudget.Date.Year
        );

        if (parentBudget == null)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["ParentBudgetNotFoundLog", parentCategory]
            );
            return;
        }

        // A parent budget cannot have a limit less than the sum of its children.
        if (parentBudget.Limit < parentBudget.Limit + childBudget.Limit)
        {
            parentBudget.Limit += childBudget.Limit;
        }
    }
}
