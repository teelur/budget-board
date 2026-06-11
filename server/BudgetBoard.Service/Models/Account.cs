using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IAccountCreateRequest
{
    public string Name { get; }
    public Guid InstitutionID { get; }
}

public class AccountCreateRequest() : IAccountCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public required Guid InstitutionID { get; set; }
}

public interface IAccountUpdateRequest
{
    public Guid ID { get; }
    public string? Name { get; }
    public string? Type { get; }
    public bool? HideTransactions { get; }
    public bool? HideAccount { get; }
    public decimal? InterestRate { get; }
    public string? Source { get; }
}

public class AccountUpdateRequest() : IAccountUpdateRequest
{
    public required Guid ID { get; set; }
    public string? Name { get; set; } = null;
    public string? Type { get; set; } = null;
    public bool? HideTransactions { get; set; } = null;
    public bool? HideAccount { get; set; } = null;
    public decimal? InterestRate { get; set; } = null;
    public string? Source { get; set; } = null;
}

public interface IAccountIndexRequest
{
    public Guid ID { get; }
    public int Index { get; }
}

public class AccountIndexRequest : IAccountIndexRequest
{
    public required Guid ID { get; set; }
    public required int Index { get; set; }
}

public interface IAccountResponse
{
    public Guid ID { get; }
    public string Name { get; }
    public Guid InstitutionID { get; }
    public string Type { get; }
    public decimal CurrentBalance { get; }
    public DateOnly? BalanceDate { get; }
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
    public Guid InstitutionID { get; set; }
    public string Type { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateOnly? BalanceDate { get; set; }
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
        ID = Guid.Empty;
        Name = string.Empty;
        InstitutionID = Guid.Empty;
        Type = string.Empty;
        CurrentBalance = 0.0M;
        BalanceDate = null;
        HideTransactions = false;
        HideAccount = false;
        Deleted = null;
        Index = 0;
        Source = string.Empty;
        UserID = Guid.Empty;
    }

    public AccountResponse(Account account)
    {
        ID = account.ID;
        Name = account.Name;
        InstitutionID = account.InstitutionID;
        Type = account.Type;
        CurrentBalance =
            account.Balances.OrderByDescending(b => b.Date).FirstOrDefault()?.Amount ?? 0;
        BalanceDate = account.Balances.OrderByDescending(b => b.Date).FirstOrDefault()?.Date;
        HideTransactions = account.HideTransactions;
        HideAccount = account.HideAccount;
        Deleted = account.Deleted;
        Index = account.Index;
        InterestRate = account.InterestRate;
        Source = account.Source;
        UserID = account.UserID;
    }
}
