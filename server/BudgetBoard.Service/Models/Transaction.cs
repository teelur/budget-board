using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public class TransactionSource
{
    private TransactionSource(string value)
    {
        Value = value;
    }

    public string Value { get; set; }

    public static TransactionSource Manual
    {
        get { return new TransactionSource("Manual"); }
    }
    public static TransactionSource SimpleFin
    {
        get { return new TransactionSource("SimpleFin"); }
    }
}

public interface ITransactionCreateRequest
{
    string? SyncID { get; }
    decimal Amount { get; }
    DateTime Date { get; }
    string? Category { get; }
    string? Subcategory { get; }
    string? MerchantName { get; }
    string? Source { get; }
    Guid AccountID { get; }
}

public class TransactionCreateRequest : ITransactionCreateRequest
{
    public string? SyncID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? MerchantName { get; set; }
    public string? Source { get; set; }
    public Guid AccountID { get; set; }

    [JsonConstructor]
    public TransactionCreateRequest()
    {
        SyncID = null;
        Amount = 0.0M;
        Date = DateTime.MinValue;
        Category = null;
        Subcategory = null;
        MerchantName = null;
        Source = string.Empty;
        AccountID = Guid.NewGuid();
    }
}

public interface ITransactionUpdateRequest
{
    Guid ID { get; }
    decimal Amount { get; }
    DateTime Date { get; }
    string? Category { get; }
    string? Subcategory { get; }
    string? MerchantName { get; }
    DateTime? Deleted { get; }
}

public class TransactionUpdateRequest : ITransactionUpdateRequest
{
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? MerchantName { get; set; }
    public DateTime? Deleted { get; set; }

    [JsonConstructor]
    public TransactionUpdateRequest()
    {
        ID = Guid.NewGuid();
        Amount = 0.0M;
        Date = DateTime.MinValue;
        Category = null;
        Subcategory = null;
        MerchantName = null;
        Deleted = null;
    }

    public TransactionUpdateRequest(Transaction transaction)
    {
        ID = transaction.ID;
        Amount = transaction.Amount;
        Date = transaction.Date;
        Category = transaction.Category;
        Subcategory = transaction.Subcategory;
        MerchantName = transaction.MerchantName;
        Deleted = transaction.Deleted;
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
    public Guid ID { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }

    [JsonConstructor]
    public TransactionSplitRequest()
    {
        ID = Guid.NewGuid();
        Amount = 0.0M;
        Category = string.Empty;
        Subcategory = string.Empty;
    }
}

public class TransactionImport
{
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal? Amount { get; set; }
    public string Account { get; set; }

    [JsonConstructor]
    public TransactionImport()
    {
        Date = DateTime.MinValue;
        Description = string.Empty;
        Category = null;
        Amount = 0.0M;
        Account = string.Empty;
    }
}

public class AccountNameToIDKeyValuePair
{
    public string AccountName { get; set; }
    public Guid AccountID { get; set; }

    [JsonConstructor]
    public AccountNameToIDKeyValuePair()
    {
        AccountName = string.Empty;
        AccountID = Guid.NewGuid();
    }
}

public interface ITransactionImportRequest
{
    public IEnumerable<TransactionImport> Transactions { get; }
    public IEnumerable<AccountNameToIDKeyValuePair> AccountNameToIDMap { get; }
}

public class TransactionImportRequest : ITransactionImportRequest
{
    public IEnumerable<TransactionImport> Transactions { get; set; }
    public IEnumerable<AccountNameToIDKeyValuePair> AccountNameToIDMap { get; set; }

    [JsonConstructor]
    public TransactionImportRequest()
    {
        Transactions = [];
        AccountNameToIDMap = [];
    }
}

public interface ITransactionResponse
{
    Guid ID { get; }
    string? SyncID { get; }
    decimal Amount { get; }
    DateTime Date { get; }
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
    public Guid ID { get; set; }
    public string? SyncID { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? MerchantName { get; set; }
    public bool Pending { get; set; }
    public DateTime? Deleted { get; set; }
    public string Source { get; set; }
    public Guid AccountID { get; set; }

    [JsonConstructor]
    public TransactionResponse()
    {
        ID = Guid.NewGuid();
        SyncID = null;
        Amount = 0.0M;
        Date = DateTime.MinValue;
        Category = null;
        Subcategory = null;
        MerchantName = null;
        Source = string.Empty;
        AccountID = Guid.NewGuid();
    }

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
