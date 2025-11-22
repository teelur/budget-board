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
    internal static string GetParentCategory(string category, IEnumerable<ICategory> categories)
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
    internal static bool GetIsParentCategory(string category, IEnumerable<ICategory> categories)
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
    internal static IReadOnlyList<ICategory> GetAllTransactionCategories(
        IEnumerable<ICategory> customCategories,
        bool disableBuiltInTransactionCategories
    )
    {
        var allCategories = new List<ICategory>();
        if (!disableBuiltInTransactionCategories)
        {
            allCategories.AddRange(TransactionCategoriesConstants.DefaultTransactionCategories);
        }
        allCategories.AddRange(customCategories);
        return allCategories;
    }
}
