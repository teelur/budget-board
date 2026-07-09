using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public struct TransactionCategoryTypes
{
    public const string Expense = "expense";
    public const string Income = "income";
    public static readonly IEnumerable<string> AllTypes = [Expense, Income];
}

public interface ITransactionCategory
{
    public string Value { get; }
    public string Parent { get; }
    public string CategoryType { get; }
}

public class TransactionCategoryBase : ITransactionCategory
{
    public string Value { get; set; }
    public string Parent { get; set; }
    public string CategoryType { get; set; }

    [JsonConstructor]
    public TransactionCategoryBase()
    {
        Value = string.Empty;
        Parent = string.Empty;
        CategoryType = TransactionCategoryTypes.Expense;
    }
}

public interface ITransactionCategoryCreateRequest : ITransactionCategory { }

public class TransactionCategoryCreateRequest : ITransactionCategoryCreateRequest
{
    public string Value { get; set; }
    public string Parent { get; set; }
    public string CategoryType { get; set; }

    [JsonConstructor]
    public TransactionCategoryCreateRequest()
    {
        Value = string.Empty;
        Parent = string.Empty;
        CategoryType = TransactionCategoryTypes.Expense;
    }
}

public interface ITransactionCategoryUpdateRequest : ITransactionCategory
{
    Guid ID { get; }
}

public class TransactionCategoryUpdateRequest : ITransactionCategoryUpdateRequest
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }
    public string CategoryType { get; set; }

    [JsonConstructor]
    public TransactionCategoryUpdateRequest()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
        CategoryType = TransactionCategoryTypes.Expense;
    }
}

public interface ITransactionCategoryResponse : ITransactionCategory
{
    Guid ID { get; }
    Guid UserID { get; }
}

public class CategoryResponse : ITransactionCategoryResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Value { get; set; } = string.Empty;
    public string Parent { get; set; } = string.Empty;
    public string CategoryType { get; set; } = TransactionCategoryTypes.Expense;
    public Guid UserID { get; set; } = Guid.NewGuid();

    public CategoryResponse(Category category)
    {
        ID = category.ID;
        Value = category.Value;
        Parent = category.Parent;
        CategoryType = category.CategoryType;
        UserID = category.UserID;
    }

    public CategoryResponse(ITransactionCategory category)
    {
        ID = Guid.Empty;
        Value = category.Value;
        Parent = category.Parent;
        CategoryType = category.CategoryType;
        UserID = Guid.Empty;
    }
}
