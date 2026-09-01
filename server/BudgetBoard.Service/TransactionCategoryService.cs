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

public class TransactionCategoryService(
    ILogger<ITransactionCategoryService> logger,
    UserDataContext userDataContext,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : ITransactionCategoryService
{
    /// <inheritdoc />
    public async Task CreateTransactionCategoryAsync(
        Guid userGuid,
        ITransactionCategoryCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var allTransactionCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            userData
        );

        ValidateTransactionCategoryData(
            request.Value,
            request.Parent,
            request.CategoryType,
            allTransactionCategories
        );

        var newCategory = new Category
        {
            Value = request.Value,
            Parent = request.Parent,
            CategoryType = ResolveCategoryType(
                request.Parent,
                request.CategoryType,
                allTransactionCategories
            ),
            UserID = userData.Id,
        };

        userDataContext.TransactionCategories.Add(newCategory);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionCategoryResponse>> ReadTransactionCategoriesAsync(
        Guid userGuid
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        return TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
    }

    /// <inheritdoc />
    public async Task UpdateTransactionCategoryAsync(
        Guid userGuid,
        ITransactionCategoryUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var transactionCategory = GetTransactionCategoryById(userData, request.ID);
        var allTransactionCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            userData
        );

        ValidateTransactionCategoryData(
            request.Value,
            request.Parent,
            request.CategoryType,
            allTransactionCategories,
            request.ID
        );

        var oldValue = transactionCategory.Value;

        transactionCategory.Value = request.Value;
        transactionCategory.Parent = request.Parent;
        transactionCategory.CategoryType = ResolveCategoryType(
            request.Parent,
            request.CategoryType,
            allTransactionCategories
        );

        UpdateTransactionsUsingCategory(
            oldValue,
            request.Value,
            userData.Accounts.SelectMany(a => a.Transactions)
        );

        UpdateBudgetsUsingCategory(oldValue, request.Value, userData.Budgets);
        UpdateRuleActionsUsingCategory(
            oldValue,
            request.Value,
            userData.AutomaticRules.SelectMany(r => r.Actions)
        );
        UpdateIconsUsingCategory(oldValue, request.Value, userData.TransactionCategoryIcons);

        UpdateChildrenCategoryType(
            userData.TransactionCategories,
            oldValue,
            transactionCategory.CategoryType
        );
        UpdateChildrenParentValue(userData.TransactionCategories, oldValue, request.Value);

        await userDataContext.SaveChangesAsync();

        static void UpdateBudgetsUsingCategory(
            string oldValue,
            string newValue,
            IEnumerable<Budget> budgets
        )
        {
            foreach (var budget in budgets)
            {
                if (budget.Category.Equals(oldValue, StringComparison.OrdinalIgnoreCase))
                    budget.Category = newValue;
            }
        }

        static void UpdateChildrenCategoryType(
            ICollection<Category> categories,
            string parentValue,
            string newCategoryType
        ) =>
            categories
                .Where(c => c.Parent.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(c => c.CategoryType = newCategoryType);

        static void UpdateChildrenParentValue(
            ICollection<Category> categories,
            string oldParentValue,
            string newParentValue
        ) =>
            categories
                .Where(c => c.Parent.Equals(oldParentValue, StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(c => c.Parent = newParentValue);
    }

    /// <inheritdoc />
    public async Task SetTransactionCategoryIconAsync(
        Guid userGuid,
        ITransactionCategoryIconUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var allTransactionCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            userData
        );

        var category = GetTransactionCategoryByValue(allTransactionCategories, request.Category);
        ThrowIfIconTooLong(request.Icon);

        var icon = userData.TransactionCategoryIcons.FirstOrDefault(i =>
            i.Category.Equals(category.Value, StringComparison.OrdinalIgnoreCase)
        );

        if (string.IsNullOrWhiteSpace(request.Icon))
        {
            if (icon != null)
                userData.TransactionCategoryIcons.Remove(icon);
        }
        else if (icon != null)
        {
            icon.Icon = request.Icon;
        }
        else
        {
            userDataContext.TransactionCategoryIcons.Add(
                new CategoryIcon
                {
                    Category = category.Value,
                    Icon = request.Icon,
                    UserID = userData.Id,
                }
            );
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ClearBuiltInTransactionCategoryReferencesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var transactions = userData.Accounts.SelectMany(a => a.Transactions);
        var builtInCategoryValues = new HashSet<string>(
            TransactionCategoriesConstants.DefaultTransactionCategories.Select(category =>
                category.Value
            ),
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var category in builtInCategoryValues)
        {
            UpdateTransactionsUsingCategory(category, null, transactions, true);
            RemoveIconsUsingCategory(category, userData.TransactionCategoryIcons);
        }

        foreach (var category in userData.TransactionCategories)
        {
            if (builtInCategoryValues.Contains(category.Parent))
            {
                category.Parent = string.Empty;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteTransactionCategoryAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var transactionCategory = GetTransactionCategoryById(userData, guid);
        var allTransactionCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            userData
        );

        RemoveChildrenUsingCategory(transactionCategory.Value);
        UpdateTransactionsUsingCategory(
            transactionCategory.Value,
            null,
            userData.Accounts.SelectMany(a => a.Transactions),
            true
        );
        RemoveBudgetsUsingCategory(
            transactionCategory.Value,
            userData.Budgets,
            allTransactionCategories
        );
        UpdateRuleActionsUsingCategory(
            transactionCategory.Value,
            null,
            userData.AutomaticRules.SelectMany(r => r.Actions)
        );
        RemoveIconsUsingCategory(transactionCategory.Value, userData.TransactionCategoryIcons);

        userData.TransactionCategories.Remove(transactionCategory);
        await userDataContext.SaveChangesAsync();

        void RemoveChildrenUsingCategory(string parentValue)
        {
            var children = userData
                .TransactionCategories.Where(c =>
                    c.Parent.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
            foreach (var child in children)
            {
                UpdateTransactionsUsingCategory(
                    child.Value,
                    null,
                    userData.Accounts.SelectMany(a => a.Transactions),
                    true
                );
                RemoveBudgetsUsingCategory(child.Value, userData.Budgets, allTransactionCategories);
                UpdateRuleActionsUsingCategory(
                    child.Value,
                    null,
                    userData.AutomaticRules.SelectMany(r => r.Actions)
                );
                RemoveIconsUsingCategory(child.Value, userData.TransactionCategoryIcons);
                userData.TransactionCategories.Remove(child);
            }
        }

        static void RemoveBudgetsUsingCategory(
            string value,
            ICollection<Budget> budgets,
            IEnumerable<ITransactionCategory> allCategories
        )
        {
            var toRemove = budgets
                .Where(b => b.Category.Equals(value, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var budget in toRemove)
                budgets.Remove(budget);
        }
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
                    .Include(u => u.TransactionCategories)
                    .Include(u => u.TransactionCategoryIcons)
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Transactions)
                    .Include(u => u.Budgets)
                    .Include(u => u.AutomaticRules)
                    .ThenInclude(r => r.Actions)
                    .Include(u => u.UserSettings)
        );
    }

    private Category GetTransactionCategoryById(ApplicationUser userData, Guid id)
    {
        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t => t.ID == id);
        if (transactionCategory == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryNotFoundError"]
            );
        }

        return transactionCategory;
    }

    private ITransactionCategoryResponse GetTransactionCategoryByValue(
        IEnumerable<ITransactionCategoryResponse> categories,
        string value
    )
    {
        var transactionCategory = categories.FirstOrDefault(c =>
            c.Value.Equals(value, StringComparison.OrdinalIgnoreCase)
        );
        if (transactionCategory == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryNotFoundError"]
            );
        }

        return transactionCategory;
    }

    private void ValidateTransactionCategoryData(
        string value,
        string parent,
        string categoryType,
        IEnumerable<ITransactionCategoryResponse> categories,
        Guid? id = null
    )
    {
        ThrowIfDuplicateName(value, id, categories);
        ThrowIfValueIsNullOrEmpty(value);
        ThrowIfValueSameNameAsParent(value, parent);
        ThrowIfParentNotFound(parent, categories);
        ThrowIfInvalidCategoryType(categoryType);
    }

    private void ThrowIfDuplicateName(
        string value,
        Guid? id,
        IEnumerable<ITransactionCategoryResponse> categories
    )
    {
        if (
            categories.Any(c =>
            {
                var valueMatches = c.Value.Equals(value, StringComparison.OrdinalIgnoreCase);
                var idMatches = id.HasValue && c.ID == id.Value;
                return valueMatches && !idMatches;
            })
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryDuplicateNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryDuplicateNameError"]
            );
        }
    }

    private void ThrowIfValueIsNullOrEmpty(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryEmptyNameLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryEmptyNameError"]
            );
        }
    }

    private void ThrowIfValueSameNameAsParent(string value, string parentValue)
    {
        if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategorySameNameAsParentLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategorySameNameAsParentError"]
            );
        }
    }

    private void ThrowIfParentNotFound(
        string parentValue,
        IEnumerable<ITransactionCategory> categories
    )
    {
        if (
            !string.IsNullOrEmpty(parentValue)
            && !categories.Any(c => c.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
        )
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryParentNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryParentNotFoundError"]
            );
        }
    }

    private void ThrowIfIconTooLong(string icon)
    {
        if (icon.Length > CategoryIcon.MaxIconLength)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryIconTooLongLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryIconTooLongError"]
            );
        }
    }

    private void ThrowIfInvalidCategoryType(string categoryType)
    {
        if (!TransactionCategoryTypes.AllTypes.Contains(categoryType))
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryInvalidTypeLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryInvalidTypeError"]
            );
        }
    }

    private static string ResolveCategoryType(
        string parent,
        string categoryType,
        IEnumerable<ITransactionCategoryResponse> allTransactionCategories
    ) =>
        string.IsNullOrEmpty(parent)
            ? categoryType
            : allTransactionCategories
                .First(a => a.Value.Equals(parent, StringComparison.OrdinalIgnoreCase))
                .CategoryType;

    private static void UpdateTransactionsUsingCategory(
        string value,
        string? replacement,
        IEnumerable<Transaction> transactions,
        bool clearBoth = false
    )
    {
        foreach (var transaction in transactions)
        {
            var categoryMatches = (transaction.Category ?? string.Empty).Equals(
                value,
                StringComparison.OrdinalIgnoreCase
            );
            var subcategoryMatches = (transaction.Subcategory ?? string.Empty).Equals(
                value,
                StringComparison.OrdinalIgnoreCase
            );
            var eitherMatches = categoryMatches || subcategoryMatches;

            if (categoryMatches || (clearBoth && eitherMatches))
            {
                transaction.Category = replacement;
            }
            if (subcategoryMatches || (clearBoth && eitherMatches))
            {
                transaction.Subcategory = replacement;
            }
        }
    }

    private static void UpdateIconsUsingCategory(
        string oldValue,
        string newValue,
        IEnumerable<CategoryIcon> icons
    )
    {
        foreach (var icon in icons)
        {
            if (icon.Category.Equals(oldValue, StringComparison.OrdinalIgnoreCase))
                icon.Category = newValue;
        }
    }

    private static void RemoveIconsUsingCategory(string value, ICollection<CategoryIcon> icons)
    {
        var toRemove = icons
            .Where(i => i.Category.Equals(value, StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var icon in toRemove)
            icons.Remove(icon);
    }

    private static void UpdateRuleActionsUsingCategory(
        string oldValue,
        string? newValue,
        IEnumerable<RuleAction> actions
    )
    {
        foreach (var action in actions)
        {
            if (
                action.Field.Equals(
                    AutomaticRuleConstants.TransactionFields.Category,
                    StringComparison.OrdinalIgnoreCase
                ) && action.Value.Equals(oldValue, StringComparison.OrdinalIgnoreCase)
            )
                action.Value = newValue ?? string.Empty;
        }
    }
}
