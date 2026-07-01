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
    /// <inheritdoc />
    public async Task CreateBudgetsAsync(
        Guid userGuid,
        IEnumerable<IBudgetCreateRequest> requests,
        bool autoManageParents = false
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        int newBudgetsCount = 0;
        List<string> errors = [];
        foreach (var request in requests)
        {
            var error = TryAddBudget(userData, request, allCategories, out var newBudget);
            if (error != null)
            {
                errors.Add(error);
                continue;
            }
            newBudgetsCount++;

            if (
                autoManageParents
                && !TransactionCategoriesHelpers.GetIsParentCategory(
                    request.Category,
                    allCategories
                )
            )
            {
                var parentCategory = TransactionCategoriesHelpers.GetParentCategory(
                    request.Category,
                    allCategories
                );

                var parentBudgetAlreadyExists = userData.Budgets.Any(b =>
                    b.Month.Month == request.Month.Month
                    && b.Month.Year == request.Month.Year
                    && b.Category.Equals(
                        parentCategory,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );

                if (parentBudgetAlreadyExists)
                {
                    UpdateParentBudgetLimit(parentCategory, newBudget!, userData);
                    continue;
                }

                var parentBudgetRequest = new BudgetCreateRequest
                {
                    Month = request.Month,
                    Category = parentCategory,
                    Limit = GetBudgetChildrenLimit(parentCategory, request.Month, userData),
                };

                error = TryAddBudget(
                    userData,
                    parentBudgetRequest,
                    allCategories,
                    out var newParentBudget
                );
                if (error != null)
                {
                    errors.Add(error);
                    continue;
                }
                newBudgetsCount++;
            }
        }

        await userDataContext.SaveChangesAsync();
        if (errors.Count > 0)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["BudgetCreateCompletedWithErrorsLog", newBudgetsCount, errors.Count]
            );
            throw new BudgetBoardServiceException(
                responseLocalizer["BudgetCreateCompletedWithErrorsError", string.Join('\n', errors)]
            );
        }
        else
        {
            logger.LogInformation(
                "{LogMessage}",
                logLocalizer["BudgetCreateCompletedLog", newBudgetsCount]
            );
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IBudgetResponse>> ReadBudgetsAsync(
        Guid userGuid,
        DateOnly month
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var budgets = userData.Budgets.Where(b =>
            b.Month.Month == month.Month && b.Month.Year == month.Year
        );

        return budgets.Select(b => new BudgetResponse(b)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = GetBudgetById(userData, request.ID);

        budget.Limit = request.Limit;

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        // If the budget being updated is a parent category, ensure that the sum of its children does not exceed the new limit.
        if (TransactionCategoriesHelpers.GetIsParentCategory(budget.Category, allCategories))
        {
            var childBudgetsLimitTotal = GetBudgetChildrenLimit(
                budget.Category,
                budget.Month,
                userData
            );

            if (BudgetHasChildren(userData, budget) && childBudgetsLimitTotal > budget.Limit)
            {
                logger.LogError("{LogMessage}", logLocalizer["BudgetUpdateParentLimitErrorLog"]);
                throw new BudgetBoardServiceException(
                    responseLocalizer["BudgetUpdateParentLimitError"]
                );
            }
        }
        // If the budget being updated is a child category, ensure that the sum of its siblings does not exceed the limit of the parent.
        else
        {
            var parentCategory = TransactionCategoriesHelpers.GetParentCategory(
                budget.Category,
                allCategories
            );

            if (string.IsNullOrWhiteSpace(parentCategory))
            {
                await userDataContext.SaveChangesAsync();
                return;
            }

            var parentBudget = userData.Budgets.SingleOrDefault(b =>
                b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
                && b.Month.Month == budget.Month.Month
                && b.Month.Year == budget.Month.Year
            );

            var childBudgetsLimitTotal = GetBudgetChildrenLimit(
                parentCategory,
                budget.Month,
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
                    Month = budget.Month,
                    Category = parentCategory,
                    Limit = childBudgetsLimitTotal,
                    UserID = userData.Id,
                };
                userDataContext.Budgets.Add(newParentBudget);
            }
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = GetBudgetById(userData, budgetGuid);

        userDataContext.Budgets.Remove(budget);

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        if (TransactionCategoriesHelpers.GetIsParentCategory(budget.Category, allCategories))
        {
            var childBudgetsForMonth = GetChildBudgetsForMonth(
                userData,
                budget.Category,
                budget.Month
            );

            foreach (var childBudget in childBudgetsForMonth)
            {
                userDataContext.Budgets.Remove(childBudget);
            }
        }

        await userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users =>
                users
                    .Include(u => u.Budgets)
                    .Include(u => u.TransactionCategories)
                    .Include(u => u.UserSettings)
        );
    }

    private Budget GetBudgetById(ApplicationUser userData, Guid budgetGuid)
    {
        var budget = userData.Budgets.SingleOrDefault(b => b.ID == budgetGuid);
        if (budget == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["BudgetNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["BudgetNotFoundError"]);
        }

        return budget;
    }

    private string? TryAddBudget(
        ApplicationUser userData,
        IBudgetCreateRequest request,
        IEnumerable<ITransactionCategoryResponse> allCategories,
        out Budget? newBudget
    )
    {
        newBudget = null;
        if (
            allCategories.All(c =>
                !c.Value.Equals(request.Category, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer["BudgetCreateCategoryNotFoundLog", request.Category]
            );
            return responseLocalizer["BudgetCreateCategoryNotFoundError", request.Category];
        }

        var budgetForCategoryAlreadyExists = userData.Budgets.Any(b =>
            b.Month.Month == request.Month.Month
            && b.Month.Year == request.Month.Year
            && b.Category.Equals(request.Category, StringComparison.InvariantCultureIgnoreCase)
        );

        if (budgetForCategoryAlreadyExists)
        {
            logger.LogWarning(
                "{LogMessage}",
                logLocalizer[
                    "BudgetCreateDuplicateLog",
                    request.Category,
                    request.Month.ToString("yyyy-MM")
                ]
            );
            return responseLocalizer[
                "BudgetCreateDuplicateError",
                request.Category,
                request.Month.ToString("yyyy-MM")
            ];
        }

        newBudget = new Budget
        {
            Month = request.Month,
            Category = request.Category,
            Limit = request.Limit,
            UserID = userData.Id,
        };

        userDataContext.Budgets.Add(newBudget);
        return null;
    }

    private static List<Budget> GetChildBudgetsForMonth(
        ApplicationUser userData,
        string parentCategory,
        DateOnly monthDate
    )
    {
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var childBudgets = userData.Budgets.Where(b =>
            !TransactionCategoriesHelpers.GetIsParentCategory(b.Category, allCategories)
            && TransactionCategoriesHelpers
                .GetParentCategory(b.Category, allCategories)
                .Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
        );

        List<Budget> childBudgetsForMonth =
        [
            .. childBudgets.Where(b =>
                b.Month.Month == monthDate.Month && b.Month.Year == monthDate.Year
            ),
        ];

        return childBudgetsForMonth;
    }

    private static bool BudgetHasChildren(ApplicationUser userData, Budget budget) =>
        GetChildBudgetsForMonth(userData, budget.Category, budget.Month).Count > 0;

    private static decimal GetBudgetChildrenLimit(
        string parentCategory,
        DateOnly date,
        ApplicationUser userData
    ) => GetChildBudgetsForMonth(userData, parentCategory, date).Sum(b => b.Limit);

    private static void UpdateParentBudgetLimit(
        string parentCategory,
        Budget childBudget,
        ApplicationUser userData
    )
    {
        var parentBudget = userData.Budgets.Single(b =>
            b.Category.Equals(parentCategory, StringComparison.CurrentCultureIgnoreCase)
            && b.Month.Month == childBudget.Month.Month
            && b.Month.Year == childBudget.Month.Year
        );

        // A parent budget cannot have a limit less than the sum of its children.
        if (parentBudget.Limit < parentBudget.Limit + childBudget.Limit)
        {
            parentBudget.Limit += childBudget.Limit;
        }
    }
}
