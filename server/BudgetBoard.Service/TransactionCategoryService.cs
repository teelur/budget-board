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
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var allTransactionCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            userData
        );

        ThrowIfDuplicateName(request.Value, allTransactionCategories);
        ThrowIfValueIsNullOrEmpty(request.Value);
        ThrowIfValueSameNameAsParent(request.Value, request.Parent);
        ThrowIfParentNotFound(request.Parent, allTransactionCategories);
        ThrowIfInvalidCategoryType(request.CategoryType);

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

        void ThrowIfDuplicateName(string value, IEnumerable<ITransactionCategory> categories)
        {
            if (categories.Any(c => c.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryCreateDuplicateNameLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryCreateDuplicateNameError"]
                );
            }
        }

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryCreateEmptyNameLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryCreateEmptyNameError"]
                );
            }
        }

        void ThrowIfValueSameNameAsParent(string value, string parentValue)
        {
            if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryCreateSameNameAsParentLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryCreateSameNameAsParentError"]
                );
            }
        }

        void ThrowIfParentNotFound(string parentValue, IEnumerable<ITransactionCategory> categories)
        {
            if (
                !string.IsNullOrEmpty(parentValue)
                && !categories.Any(c =>
                    c.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryCreateParentNotFoundLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryCreateParentNotFoundError"]
                );
            }
        }

        string ResolveCategoryType(
            string parent,
            string categoryType,
            IEnumerable<ITransactionCategoryResponse> allCategoryTypes
        ) =>
            string.IsNullOrEmpty(parent)
                ? categoryType
                : allCategoryTypes
                    .First(a => a.Value.Equals(parent, StringComparison.OrdinalIgnoreCase))
                    .CategoryType;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ITransactionCategoryResponse>> ReadTransactionCategoriesAsync(
        Guid userGuid
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        return TransactionCategoriesHelpers.GetAllTransactionCategories(userData);
    }

    /// <inheritdoc />
    public async Task UpdateTransactionCategoryAsync(
        Guid userGuid,
        ITransactionCategoryUpdateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t =>
            t.ID == request.ID
        );
        if (transactionCategory == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryUpdateNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryUpdateNotFoundError"]
            );
        }

        var allTransactionCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            userData
        );

        ThrowIfDuplicateName(request.Value, allTransactionCategories);
        ThrowIfValueIsNullOrEmpty(request.Value);
        ThrowIfValueSameNameAsParent(request.Value, request.Parent);
        ThrowIfParentNotFound(request.Parent, allTransactionCategories);
        ThrowIfInvalidCategoryType(request.CategoryType);

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

        UpdateChildrenCategoryType(
            userData.TransactionCategories,
            request.Value,
            transactionCategory.CategoryType
        );
        UpdateChildrenParentValue(userData.TransactionCategories, oldValue, request.Value);

        await userDataContext.SaveChangesAsync();

        void ThrowIfDuplicateName(
            string value,
            IEnumerable<ITransactionCategoryResponse> categories
        )
        {
            if (
                categories.Any(c =>
                    c.ID != request.ID && c.Value.Equals(value, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryUpdateDuplicateNameLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryUpdateDuplicateNameError"]
                );
            }
        }

        void ThrowIfValueIsNullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryUpdateEmptyNameLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryUpdateEmptyNameError"]
                );
            }
        }

        void ThrowIfValueSameNameAsParent(string value, string parentValue)
        {
            if (value.Equals(parentValue, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryUpdateSameNameAsParentLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryUpdateSameNameAsParentError"]
                );
            }
        }

        void ThrowIfParentNotFound(string parentValue, IEnumerable<ITransactionCategory> categories)
        {
            if (
                !string.IsNullOrEmpty(parentValue)
                && !categories.Any(c =>
                    c.Value.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                logger.LogError(
                    "{LogMessage}",
                    logLocalizer["TransactionCategoryUpdateParentNotFoundLog"]
                );
                throw new BudgetBoardServiceException(
                    responseLocalizer["TransactionCategoryUpdateParentNotFoundError"]
                );
            }
        }

        string ResolveCategoryType(
            string parent,
            string categoryType,
            IEnumerable<ITransactionCategoryResponse> allTransactionCategories
        ) =>
            string.IsNullOrEmpty(parent)
                ? categoryType
                : allTransactionCategories
                    .First(a => a.Value.Equals(parent, StringComparison.OrdinalIgnoreCase))
                    .CategoryType;

        static void UpdateTransactionsUsingCategory(
            string oldValue,
            string newValue,
            IEnumerable<Transaction> transactions
        )
        {
            foreach (var transaction in transactions)
            {
                if (
                    (transaction.Category ?? string.Empty).Equals(
                        oldValue,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    transaction.Category = newValue;

                if (
                    (transaction.Subcategory ?? string.Empty).Equals(
                        oldValue,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    transaction.Subcategory = newValue;
            }
        }

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
        )
        {
            foreach (
                var child in categories.Where(c =>
                    c.Parent.Equals(parentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                child.CategoryType = newCategoryType;
            }
        }

        static void UpdateChildrenParentValue(
            ICollection<Category> categories,
            string oldParentValue,
            string newParentValue
        )
        {
            foreach (
                var child in categories.Where(c =>
                    c.Parent.Equals(oldParentValue, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                child.Parent = newParentValue;
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteTransactionCategoryAsync(Guid userGuid, Guid guid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var transactionCategory = userData.TransactionCategories.FirstOrDefault(t => t.ID == guid);
        if (transactionCategory == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["TransactionCategoryDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                responseLocalizer["TransactionCategoryDeleteNotFoundError"]
            );
        }

        RemoveChildrenUsingCategory(transactionCategory.Value);
        NullOutTransactionsUsingCategory(
            transactionCategory.Value,
            userData.Accounts.SelectMany(a => a.Transactions)
        );
        RemoveBudgetsUsingCategory(transactionCategory.Value, userData.Budgets);
        UpdateRuleActionsUsingCategory(
            transactionCategory.Value,
            null,
            userData.AutomaticRules.SelectMany(r => r.Actions)
        );

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
                NullOutTransactionsUsingCategory(
                    child.Value,
                    userData.Accounts.SelectMany(a => a.Transactions)
                );
                RemoveBudgetsUsingCategory(child.Value, userData.Budgets);
                UpdateRuleActionsUsingCategory(
                    child.Value,
                    null,
                    userData.AutomaticRules.SelectMany(r => r.Actions)
                );
                userData.TransactionCategories.Remove(child);
            }
        }

        static void NullOutTransactionsUsingCategory(
            string value,
            IEnumerable<Transaction> transactions
        )
        {
            foreach (var transaction in transactions)
            {
                if (
                    (transaction.Category ?? string.Empty).Equals(
                        value,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    transaction.Category = null;

                if (
                    (transaction.Subcategory ?? string.Empty).Equals(
                        value,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    transaction.Subcategory = null;
            }
        }

        static void RemoveBudgetsUsingCategory(string value, ICollection<Budget> budgets)
        {
            var toRemove = budgets
                .Where(b => b.Category.Equals(value, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var budget in toRemove)
                budgets.Remove(budget);
        }
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

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await userDataContext
                .ApplicationUsers.Include(u => u.TransactionCategories)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.Budgets)
                .Include(u => u.AutomaticRules)
                .ThenInclude(r => r.Actions)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            logger.LogError("{LogMessage}", logLocalizer["UserDataRetrievalErrorLog", ex.Message]);
            throw new BudgetBoardServiceException(responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["InvalidUserErrorLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
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
}
