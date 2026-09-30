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
        var userData = await GetCurrentUserAsync(userGuid);
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
        var userData = await GetCurrentUserAsync(userGuid);

        var budgets = userData
            .Budgets.Where(b => b.Month.Month == month.Month && b.Month.Year == month.Year)
            .ToList();

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        // Only child categories roll over; a parent's rollover is the sum of its children's.
        var rolloverBudgets = budgets
            .Where(b =>
                b.RolloverStartMonth.HasValue
                && !TransactionCategoriesHelpers.GetIsParentCategory(b.Category, allCategories)
            )
            .ToList();

        if (rolloverBudgets.Count == 0)
        {
            return budgets
                .Select(b => new BudgetResponse(b) { RolloverStartMonth = null })
                .ToList();
        }

        var budgetsByCategoryMonth = userData
            .Budgets.GroupBy(b => GetCategoryMonthKey(b.Category, b.Month))
            .ToDictionary(g => g.Key, g => g.First());

        // Rollover can only accumulate from months that have budgets, starting no earlier than the
        // earliest start month, and only from months before the one being read.
        var earliestBudgetMonth = userData.Budgets.Min(b => b.Month);
        var earliestStartMonth = rolloverBudgets.Min(b => b.RolloverStartMonth!.Value);
        var spendFrom = StartOfMonth(
            earliestStartMonth > earliestBudgetMonth ? earliestStartMonth : earliestBudgetMonth
        );
        var spendTo = StartOfMonth(month).AddMonths(-1);
        var categorySpendMap =
            spendFrom <= spendTo
                ? await BuildCategorySpendMapAsync(userData.Id, spendFrom, spendTo)
                : [];

        var childRollovers = rolloverBudgets.ToDictionary(
            b => b.ID,
            b =>
                CalculateRollover(
                    b,
                    earliestBudgetMonth,
                    budgetsByCategoryMonth,
                    categorySpendMap,
                    allCategories
                )
        );

        return budgets
            .Select(b =>
            {
                if (!TransactionCategoriesHelpers.GetIsParentCategory(b.Category, allCategories))
                {
                    return new BudgetResponse(b, childRollovers.GetValueOrDefault(b.ID));
                }

                var childrenRollover = rolloverBudgets
                    .Where(child =>
                        TransactionCategoriesHelpers
                            .GetParentCategory(child.Category, allCategories)
                            .Equals(b.Category, StringComparison.InvariantCultureIgnoreCase)
                    )
                    .Sum(child => childRollovers[child.ID]);

                return new BudgetResponse(b, childrenRollover) { RolloverStartMonth = null };
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var budget = GetBudgetById(userData, request.ID);

        budget.Limit = request.Limit;

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
        var isParent = TransactionCategoriesHelpers.GetIsParentCategory(
            budget.Category,
            allCategories
        );

        // Only child categories roll over.
        var rolloverStartMonth = isParent ? null : NormalizeToMonth(request.RolloverStartMonth);
        if (rolloverStartMonth != budget.RolloverStartMonth)
        {
            // Rollover is a standing setting for the category, so it carries forward to later months.
            var budgetMonth = NormalizeToMonth(budget.Month);
            foreach (
                var categoryBudget in userData.Budgets.Where(b =>
                    b.Category.Equals(budget.Category, StringComparison.InvariantCultureIgnoreCase)
                    && NormalizeToMonth(b.Month) >= budgetMonth
                )
            )
            {
                categoryBudget.RolloverStartMonth = rolloverStartMonth;
            }
        }

        // If the budget being updated is a parent category, ensure that the sum of its children does not exceed the new limit.
        if (isParent)
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
        var userData = await GetCurrentUserAsync(userGuid);
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
            // Only child categories roll over.
            RolloverStartMonth = TransactionCategoriesHelpers.GetIsParentCategory(
                request.Category,
                allCategories
            )
                ? null
                : NormalizeToMonth(request.RolloverStartMonth)
                    ?? GetInheritedRolloverStartMonth(userData, request.Category, request.Month),
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

    private static (string Category, int Year, int Month) GetCategoryMonthKey(
        string category,
        DateOnly month
    ) => (category.ToLowerInvariant(), month.Year, month.Month);

    private static DateOnly? NormalizeToMonth(DateOnly? date) =>
        date.HasValue ? StartOfMonth(date.Value) : null;

    private static DateOnly StartOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    /// <summary>
    /// A new budget keeps the rollover setting of the most recent earlier budget for its category,
    /// so enabling rollover once applies to every month after it until it is disabled.
    /// </summary>
    private static DateOnly? GetInheritedRolloverStartMonth(
        ApplicationUser userData,
        string category,
        DateOnly month
    ) =>
        userData
            .Budgets.Where(b =>
                b.Category.Equals(category, StringComparison.InvariantCultureIgnoreCase)
                && NormalizeToMonth(b.Month) < NormalizeToMonth(month)
            )
            .OrderByDescending(b => b.Month)
            .FirstOrDefault()
            ?.RolloverStartMonth;

    /// <summary>
    /// Totals transaction amounts per category and month, mirroring how the budgets page
    /// attributes spending: a transaction counts toward both its category and its subcategory.
    /// Hidden accounts, deleted transactions and the hide-from-budgets category are excluded.
    /// </summary>
    private async Task<Dictionary<(string, int, int), decimal>> BuildCategorySpendMapAsync(
        Guid userId,
        DateOnly fromMonth,
        DateOnly toMonth
    )
    {
        var start = new DateOnly(fromMonth.Year, fromMonth.Month, 1);
        var end = new DateOnly(toMonth.Year, toMonth.Month, 1).AddMonths(1).AddDays(-1);

        var transactions = await userDataContext
            .Transactions.Where(t =>
                t.Account!.UserID == userId
                && t.Account.HideTransactions == false
                && t.Deleted == null
                && t.Category != TransactionCategoriesConstants.HideFromBudgetsCategory
                && t.Date >= start
                && t.Date <= end
            )
            .Select(t => new
            {
                t.Category,
                t.Subcategory,
                t.Date,
                t.Amount,
            })
            .ToListAsync();

        var spendMap = new Dictionary<(string, int, int), decimal>();

        foreach (var transaction in transactions)
        {
            var category = transaction.Category ?? string.Empty;
            AddSpend(spendMap, category, transaction.Date, transaction.Amount);

            var subcategory = transaction.Subcategory;
            if (
                !string.IsNullOrEmpty(subcategory)
                && !subcategory.Equals(category, StringComparison.InvariantCultureIgnoreCase)
            )
            {
                AddSpend(spendMap, subcategory, transaction.Date, transaction.Amount);
            }
        }

        return spendMap;
    }

    private static void AddSpend(
        Dictionary<(string, int, int), decimal> spendMap,
        string category,
        DateOnly date,
        decimal amount
    )
    {
        var key = GetCategoryMonthKey(category, date);
        spendMap[key] = spendMap.GetValueOrDefault(key) + amount;
    }

    /// <summary>
    /// Sums the unspent remainder of every month from the budget's rollover start month up to
    /// (but excluding) its own month. Overspent months contribute a negative amount, so a deficit
    /// carries forward until it is paid back. Months with no budget for the category are skipped,
    /// so spending in an unbudgeted month does not count against the balance.
    /// </summary>
    private static decimal CalculateRollover(
        Budget budget,
        DateOnly earliestBudgetMonth,
        IReadOnlyDictionary<(string, int, int), Budget> budgetsByCategoryMonth,
        IReadOnlyDictionary<(string, int, int), decimal> categorySpendMap,
        IEnumerable<ITransactionCategoryResponse> allCategories
    )
    {
        if (budget.RolloverStartMonth is not DateOnly startMonth)
        {
            return 0;
        }

        // Expense amounts are stored negative, so they need flipping to count against a limit.
        var sign = IsIncomeCategory(budget.Category, allCategories) ? 1 : -1;

        // Nothing exists before the first budget, so starting earlier just adds empty months.
        var effectiveStart = startMonth > earliestBudgetMonth ? startMonth : earliestBudgetMonth;
        var month = new DateOnly(effectiveStart.Year, effectiveStart.Month, 1);

        var rollover = 0m;

        while (month < budget.Month)
        {
            var key = GetCategoryMonthKey(budget.Category, month);
            if (budgetsByCategoryMonth.TryGetValue(key, out var priorBudget))
            {
                rollover += priorBudget.Limit - (categorySpendMap.GetValueOrDefault(key) * sign);
            }

            month = month.AddMonths(1);
        }

        return rollover;
    }

    private static bool IsIncomeCategory(
        string category,
        IEnumerable<ITransactionCategoryResponse> allCategories
    ) =>
        allCategories
            .FirstOrDefault(c =>
                c.Value.Equals(category, StringComparison.InvariantCultureIgnoreCase)
            )
            ?.CategoryType.Equals(
                TransactionCategoryTypes.Income,
                StringComparison.InvariantCultureIgnoreCase
            ) ?? false;

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
