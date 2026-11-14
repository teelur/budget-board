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

public class BudgetImportRequest
{
    public DateOnly Month { get; set; }
    public List<BudgetImportItemRequest> Items { get; set; } = [];
    public List<BudgetImportCategoryCreateRequest> NewCategories { get; set; } = [];
    public bool OverwriteExisting { get; set; } = true;
}
