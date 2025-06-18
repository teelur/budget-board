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
    public static string GetParentCategory(
        string category,
        IEnumerable<ICategory> customCategories
    ) =>
        GetAllCategories(customCategories)
            .FirstOrDefault(c =>
                c.Value.Equals(category, StringComparison.CurrentCultureIgnoreCase)
            )
            ?.Parent ?? string.Empty;

    /// <summary>
    /// Determines whether the specified category is a parent category.
    /// A parent category is one that has at least one other category referencing it as its parent.
    /// </summary>
    /// <param name="category">The category name to check.</param>
    /// <param name="customCategories">A collection of custom categories to include in the search.</param>
    /// <returns>
    /// True if the specified category is a parent category; otherwise, false.
    /// </returns>
    public static bool GetIsParentCategory(
        string category,
        IEnumerable<ICategory> customCategories
    ) =>
        GetAllCategories(customCategories)
            .Any(c => c.Parent.Equals(category, StringComparison.CurrentCultureIgnoreCase));

    private static IList<ICategory> GetAllCategories(IEnumerable<ICategory> customCategories) =>
        [.. TransactionCategoriesConstants.DefaultTransactionCategories, .. customCategories];
}
