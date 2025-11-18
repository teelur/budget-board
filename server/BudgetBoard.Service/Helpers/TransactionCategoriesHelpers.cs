using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

public static class TransactionCategoriesHelpers
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
    public static string GetParentCategory(string category, IEnumerable<ICategory> categories)
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
    public static bool GetIsParentCategory(string category, IEnumerable<ICategory> categories)
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
}
