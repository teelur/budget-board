using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Resources;
using BudgetBoard.Service.Resources;

namespace BudgetBoard.Service.Models;

public class BudgetTemplateCategoryItem
{
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = TransactionCategoryTypes.Expense;
    public decimal Limit { get; set; }
    public bool RolloverEnabled { get; set; }
    public string? Icon { get; set; }
    public List<BudgetTemplateCategoryItem> Subcategories { get; set; } = [];
}

public class BudgetTemplateExport
{
    public int Version { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public List<BudgetTemplateCategoryItem> Categories { get; set; } = [];
}

public class BudgetImportCategoryCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Parent { get; set; }
    public string CategoryType { get; set; } = TransactionCategoryTypes.Expense;
    public string? Icon { get; set; }
}

public class BudgetImportItemRequest
{
    public string Category { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public bool RolloverEnabled { get; set; }
    public string? Icon { get; set; }
}

public class BudgetImportRequest : IValidatableObject
{
    private static readonly ResourceManager ResourceManager = new(
        typeof(ResponseStrings).FullName!,
        typeof(ResponseStrings).Assembly
    );

    public DateOnly Month { get; set; }
    [Required]
    public List<BudgetImportItemRequest> Items { get; set; } = [];
    public List<BudgetImportCategoryCreateRequest> NewCategories { get; set; } = [];
    public bool OverwriteExisting { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Items?.Any(item => item is null) == true)
        {
            yield return new ValidationResult(
                GetValidationError("BudgetImportItemsNullError"),
                [nameof(Items)]
            );
        }

        if (NewCategories?.Any(category => category is null) == true)
        {
            yield return new ValidationResult(
                GetValidationError("BudgetImportNewCategoriesNullError"),
                [nameof(NewCategories)]
            );
        }
    }

    private static string GetValidationError(string resourceName) =>
        ResourceManager.GetString(resourceName, CultureInfo.CurrentUICulture)!;
}
