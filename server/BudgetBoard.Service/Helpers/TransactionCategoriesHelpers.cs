using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

internal static class TransactionCategoriesHelpers
{
    /// <summary>
    /// Retrieves the parent category name for a given category.
    /// </summary>
    /// <param name="category">
    /// The name of the category whose parent is to be found.
    /// </param>
    /// <param name="customCategories">
    /// A collection of custom categories to include in the search.
    /// </param>
    /// <returns>
    /// The name of the parent category if found; otherwise, an empty string.
    /// </returns>
    internal static string GetParentCategory(
        string category,
        IEnumerable<ITransactionCategory> categories
    )
    {
        var foundCategory = categories.FirstOrDefault(c =>
            c.Value.Equals(category, StringComparison.CurrentCultureIgnoreCase)
        );

        if (foundCategory != null)
        {
            return string.IsNullOrEmpty(foundCategory.Parent)
                ? foundCategory.Value
                : foundCategory.Parent;
        }

        return string.Empty;
    }

    /// <summary>
    /// Determines whether the specified category is a parent category.
    /// A parent category is one that has at least one other category referencing it as its parent.
    /// </summary>
    /// <param name="category">The category name to check.</param>
    /// <param name="customCategories">A collection of custom categories to include in the search.</param>
    /// <returns>
    /// True if the specified category is a parent category; otherwise, false.
    /// </returns>
    internal static bool GetIsParentCategory(
        string category,
        IEnumerable<ITransactionCategory> categories
    )
    {
        if (string.IsNullOrEmpty(category))
        {
            // Empty category is Uncategorized, which should be counted as a parent.
            return true;
        }

        var foundCategory = categories.FirstOrDefault(c =>
            c.Value.Equals(category, StringComparison.CurrentCultureIgnoreCase)
        );
        if (foundCategory != null)
        {
            return categories.Any(c =>
                c.Parent.Equals(foundCategory.Value, StringComparison.CurrentCultureIgnoreCase)
            );
        }
        return false;
    }

    /// <summary>
    /// Retrieves the category and subcategory (if any) for a given category.
    /// </summary>
    /// <param name="category">
    /// The name of the category whose parent is to be found.
    /// </param>
    /// <param name="allCategories">
    /// A collection of categories to include in the search.
    /// </param>
    /// <returns>
    /// A tuple with the parent and child categories. Child may be empty.
    /// </returns>
    internal static (string parent, string child) GetFullCategory(
        string category,
        IEnumerable<ITransactionCategory> categories
    )
    {
        var parentCategory = GetParentCategory(category, categories);
        var childCategory = GetIsParentCategory(category, categories) ? string.Empty : category;
        return (parentCategory, childCategory);
    }

    /// <summary>
    /// Combines built-in and custom transaction categories based on the specified settings.
    /// </summary>
    /// <param name="customCategories">
    /// A collection of custom transaction categories to include in the combined list.
    /// </param>
    /// <param name="disableBuiltInTransactionCategories">
    /// A flag indicating whether to exclude built-in transaction categories.
    /// </param>
    /// <returns>
    /// A read-only list containing all applicable categories.
    /// </returns>
    internal static IReadOnlyList<ITransactionCategoryResponse> GetAllTransactionCategories(
        ApplicationUser userData
    )
    {
        var allTransactionCategories = new List<ITransactionCategoryResponse>();
        allTransactionCategories.AddRange(
            userData.TransactionCategories.Select(tc => new CategoryResponse(tc)).ToList()
        );

        // Special categories are always included, regardless of user settings.
        allTransactionCategories.AddRange(
            TransactionCategoriesConstants
                .SpecialTransactionCategories.Select(tc => new CategoryResponse(tc))
                .ToList()
        );

        if (userData.UserSettings?.DisableBuiltInTransactionCategories != true)
        {
            allTransactionCategories.AddRange(
                TransactionCategoriesConstants
                    .DefaultTransactionCategories.Select(tc => new CategoryResponse(tc))
                    .ToList()
            );
        }
        return allTransactionCategories;
    }
}
