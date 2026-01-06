using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public static class AccountSource
{
    public const string Manual = "Manual";
    public const string SimpleFIN = "SimpleFIN";
    public const string LunchFlow = "LunchFlow";
}

public interface IAccountCreateRequest
{
    public string Name { get; }
    public Guid? InstitutionID { get; }
    public string Type { get; }
    public string Subtype { get; }
    public bool HideTransactions { get; }
    public bool HideAccount { get; }
    public string Source { get; }
}

public class AccountCreateRequest() : IAccountCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? InstitutionID { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public bool HideTransactions { get; set; } = false;
    public bool HideAccount { get; set; } = false;
    public string Source { get; set; } = string.Empty;
}

public interface IAccountUpdateRequest
{
    public Guid ID { get; }
    public string Name { get; }
    public string Type { get; }
    public string Subtype { get; }
    public bool HideTransactions { get; }
    public bool HideAccount { get; }
    public decimal? InterestRate { get; }
}

public class AccountUpdateRequest() : IAccountUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public bool HideTransactions { get; set; } = false;
    public bool HideAccount { get; set; } = false;
    public decimal? InterestRate { get; set; } = null;

    public AccountUpdateRequest(Account account)
        : this()
    {
        ID = account.ID;
        Name = account.Name;
        Type = account.Type;
        Subtype = account.Subtype;
        HideTransactions = account.HideTransactions;
        HideAccount = account.HideAccount;
        InterestRate = account.InterestRate;
    }
}

public interface IAccountIndexRequest
{
    public Guid ID { get; }
    public int Index { get; }
}

public class AccountIndexRequest : IAccountIndexRequest
{
    public Guid ID { get; set; }
    public int Index { get; set; }
}

public interface IAccountResponse
{
    public Guid ID { get; }
    public string Name { get; }
    public Guid? InstitutionID { get; }
    public string Type { get; }
    public string Subtype { get; }
    public decimal CurrentBalance { get; }
    public DateTime? BalanceDate { get; }
    public bool HideTransactions { get; }
    public bool HideAccount { get; }
    public DateTime? Deleted { get; }
    public int Index { get; }
    public decimal InterestRate { get; }
    public string Source { get; }
    public Guid UserID { get; }
}

public class AccountResponse : IAccountResponse
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public Guid? InstitutionID { get; set; }
    public string Type { get; set; }
    public string Subtype { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? BalanceDate { get; set; }
    public bool HideTransactions { get; set; }
    public bool HideAccount { get; set; }
    public DateTime? Deleted { get; set; }
    public int Index { get; set; }
    public decimal InterestRate { get; set; }
    public string Source { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public AccountResponse()
    {
        ID = Guid.NewGuid();
        Name = string.Empty;
        InstitutionID = null;
        Type = string.Empty;
        Subtype = string.Empty;
        CurrentBalance = 0.0M;
        BalanceDate = null;
        HideTransactions = false;
        HideAccount = false;
        Deleted = null;
        Index = 0;
        Source = string.Empty;
        UserID = Guid.NewGuid();
    }

    public AccountResponse(Account account)
    {
        ID = account.ID;
        Name = account.Name;
        InstitutionID = account.InstitutionID;
        Type = account.Type;
        Subtype = account.Subtype;
        CurrentBalance =
            account
                .Balances.OrderByDescending(b => b.DateTime)
                .FirstOrDefault(b => b.Deleted == null)
                ?.Amount
            ?? 0;
        BalanceDate = account
            .Balances.OrderByDescending(b => b.DateTime)
            .FirstOrDefault(b => b.Deleted == null)
            ?.DateTime;
        HideTransactions = account.HideTransactions;
        HideAccount = account.HideAccount;
        Deleted = account.Deleted;
        Index = account.Index;
        InterestRate = account.InterestRate ?? 0;
        Source = account.Source;
        UserID = account.UserID;
    }
}
