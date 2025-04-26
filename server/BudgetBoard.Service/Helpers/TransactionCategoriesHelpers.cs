using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

public static class TransactionCategoriesHelpers
{
    public static string GetParentCategory(string category, IEnumerable<ICategory> customCategories)
    {
        var allCategories = TransactionCategoriesConstants.DefaultTransactionCategories
            .Concat(customCategories)
            .ToList();
        var parentCategory = allCategories
            .FirstOrDefault(c => c.Value.Equals(category, StringComparison.CurrentCultureIgnoreCase))?.Parent;
        return parentCategory ?? string.Empty;
    }

    public static bool GetIsParentCategory(string category, IEnumerable<ICategory> customCategories)
    {
        var allCategories = TransactionCategoriesConstants.DefaultTransactionCategories
            .Concat(customCategories)
            .ToList();
        return allCategories
            .Any(c => c.Parent.Equals(category, StringComparison.CurrentCultureIgnoreCase));
    }
}
