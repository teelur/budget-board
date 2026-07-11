using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface ITransactionCreateRequest
{
    string? SyncID { get; }
    decimal Amount { get; }
    DateOnly Date { get; }
    string? Category { get; }
    string? Subcategory { get; }
    string? MerchantName { get; }
    string? Source { get; }
    Guid AccountID { get; }
}

public class TransactionCreateRequest : ITransactionCreateRequest
{
    public string? SyncID { get; set; } = null;
    public decimal Amount { get; set; } = 0.0M;
    public DateOnly Date { get; set; } = DateOnly.MinValue;
    public string? Category { get; set; } = null;
    public string? Subcategory { get; set; } = null;
    public string? MerchantName { get; set; } = null;
    public string? Source { get; set; } = null;
    public Guid AccountID { get; set; } = Guid.NewGuid();
}

public interface ITransactionUpdateRequest
{
    Guid ID { get; }
    decimal? Amount { get; }
    DateOnly? Date { get; }
    OptionalField<string?> Category { get; }
    OptionalField<string?> Subcategory { get; }
    OptionalField<string?> MerchantName { get; }
    OptionalField<DateTime?> Deleted { get; }
}

public class TransactionUpdateRequest() : ITransactionUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public decimal? Amount { get; set; } = null;
    public DateOnly? Date { get; set; } = null;
    public OptionalField<string?> Category { get; set; } = new OptionalField<string?>();
    public OptionalField<string?> Subcategory { get; set; } = new OptionalField<string?>();
    public OptionalField<string?> MerchantName { get; set; } = new OptionalField<string?>();
    public OptionalField<DateTime?> Deleted { get; set; } = new OptionalField<DateTime?>();

    public TransactionUpdateRequest(Transaction transaction)
        : this()
    {
        ID = transaction.ID;
        Amount = transaction.Amount;
        Date = transaction.Date;
        Category = new OptionalField<string?>(transaction.Category);
        Subcategory = new OptionalField<string?>(transaction.Subcategory);
        MerchantName = new OptionalField<string?>(transaction.MerchantName);
        Deleted = new OptionalField<DateTime?>(transaction.Deleted);
    }
}

public interface ITransactionSplitRequest
{
    public Guid ID { get; }
    public decimal Amount { get; }
    public string Category { get; }
    public string Subcategory { get; }
}

public class TransactionSplitRequest : ITransactionSplitRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; } = 0.0M;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
}

public class TransactionImport
{
    public DateOnly? Date { get; set; } = null;
    public string? MerchantName { get; set; } = null;
    public string? Category { get; set; } = null;
    public decimal? Amount { get; set; } = null;
    public string Account { get; set; } = string.Empty;
}

public class AccountNameToIDKeyValuePair
{
    public string AccountName { get; set; } = string.Empty;
    public Guid AccountID { get; set; } = Guid.NewGuid();
}

public interface ITransactionImportRequest
{
    public IEnumerable<TransactionImport> Transactions { get; }
    public IEnumerable<AccountNameToIDKeyValuePair> AccountNameToIDMap { get; }
}

public class TransactionImportRequest : ITransactionImportRequest
{
    public IEnumerable<TransactionImport> Transactions { get; set; } = [];
    public IEnumerable<AccountNameToIDKeyValuePair> AccountNameToIDMap { get; set; } = [];
}

public interface ITransactionResponse
{
    Guid ID { get; }
    string? SyncID { get; }
    decimal Amount { get; }
    DateOnly Date { get; }
    string? Category { get; }
    string? Subcategory { get; }
    string? MerchantName { get; }
    bool Pending { get; }
    DateTime? Deleted { get; }
    string Source { get; }
    Guid AccountID { get; }
}

public class TransactionResponse : ITransactionResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string? SyncID { get; set; } = null;
    public decimal Amount { get; set; } = 0.0M;
    public DateOnly Date { get; set; } = DateOnly.MinValue;
    public string? Category { get; set; } = null;
    public string? Subcategory { get; set; } = null;
    public string? MerchantName { get; set; } = null;
    public bool Pending { get; set; } = false;
    public DateTime? Deleted { get; set; } = null;
    public string Source { get; set; } = string.Empty;
    public Guid AccountID { get; set; } = Guid.NewGuid();

    public TransactionResponse(Transaction transaction)
    {
        ID = transaction.ID;
        SyncID = transaction.SyncID;
        Amount = transaction.Amount;
        Date = transaction.Date;
        Category = transaction.Category;
        Subcategory = transaction.Subcategory;
        MerchantName = transaction.MerchantName;
        Deleted = transaction.Deleted;
        Source = transaction.Source;
        AccountID = transaction.AccountID;
    }
}
