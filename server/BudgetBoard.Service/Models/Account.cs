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

public class AccountResponse(Account account) : IAccountResponse
{
    public Guid ID { get; set; } = account.ID;
    public string Name { get; set; } = account.Name;
    public Guid InstitutionID { get; set; } = account.InstitutionID;
    public string Type { get; set; } = account.Type;
    public decimal CurrentBalance { get; set; } =
        account.Balances.OrderByDescending(b => b.Date).FirstOrDefault()?.Amount ?? 0;
    public DateOnly? BalanceDate { get; set; } =
        account.Balances.OrderByDescending(b => b.Date).FirstOrDefault()?.Date;
    public bool HideTransactions { get; set; } = account.HideTransactions;
    public bool HideAccount { get; set; } = account.HideAccount;
    public DateTime? Deleted { get; set; } = account.Deleted;
    public int Index { get; set; } = account.Index;
    public decimal InterestRate { get; set; } = account.InterestRate;
    public string Source { get; set; } = account.Source;
    public Guid UserID { get; set; } = account.UserID;
}

public interface IAccountUpdateRequest
{
    public Guid ID { get; }
    public OptionalField<string> Name { get; }
    public OptionalField<string> Type { get; }
    public OptionalField<bool> HideTransactions { get; }
    public OptionalField<bool> HideAccount { get; }
    public OptionalField<decimal> InterestRate { get; }
    public OptionalField<string> Source { get; }
}

public class AccountUpdateRequest() : IAccountUpdateRequest
{
    public required Guid ID { get; set; }
    public OptionalField<string> Name { get; set; }
    public OptionalField<string> Type { get; set; }
    public OptionalField<bool> HideTransactions { get; set; }
    public OptionalField<bool> HideAccount { get; set; }
    public OptionalField<decimal> InterestRate { get; set; }
    public OptionalField<string> Source { get; set; }
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
