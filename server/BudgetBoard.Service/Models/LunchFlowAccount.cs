using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface ILunchFlowAccountCreateRequest
{
    string Name { get; }
    string SyncID { get; }
    string InstitutionName { get; }
    string InstitutionLogo { get; }
    string Provider { get; }
    string? Currency { get; }
    string? Status { get; }
    decimal Balance { get; }
    int BalanceDate { get; }
    DateTime? LastSync { get; }
    Guid? LinkedAccountId { get; }
}

public class LunchFlowAccountCreateRequest : ILunchFlowAccountCreateRequest
{
    public string Name { get; init; } = string.Empty;
    public string SyncID { get; init; } = string.Empty;
    public string InstitutionName { get; init; } = string.Empty;
    public string InstitutionLogo { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Currency { get; init; } = string.Empty;
    public string? Status { get; init; } = string.Empty;
    public decimal Balance { get; init; } = 0;
    public int BalanceDate { get; init; } = (int)DateTimeOffset.UnixEpoch.ToUnixTimeSeconds();
    public DateTime? LastSync { get; init; } = null;
    public Guid? LinkedAccountId { get; init; } = null;
}

public interface ILunchFlowAccountResponse
{
    Guid ID { get; }
    string Name { get; }
    string SyncID { get; }
    string InstitutionName { get; }
    string InstitutionLogo { get; }
    string Provider { get; }
    string Currency { get; }
    string Status { get; }
    decimal Balance { get; }
    int BalanceDate { get; }
    DateTime? LastSync { get; }
    Guid? LinkedAccountId { get; }
}

public class LunchFlowAccountResponse(LunchFlowAccount lunchFlowAccount) : ILunchFlowAccountResponse
{
    public Guid ID { get; init; } = lunchFlowAccount.ID;
    public string Name { get; init; } = lunchFlowAccount.Name;
    public string SyncID { get; init; } = lunchFlowAccount.SyncID;
    public string InstitutionName { get; init; } = lunchFlowAccount.InstitutionName;
    public string InstitutionLogo { get; init; } = lunchFlowAccount.InstitutionLogo;
    public string Provider { get; init; } = lunchFlowAccount.Provider;
    public string Currency { get; init; } = lunchFlowAccount.Currency;
    public string Status { get; init; } = lunchFlowAccount.Status;
    public decimal Balance { get; init; } = lunchFlowAccount.Balance;
    public int BalanceDate { get; init; } = lunchFlowAccount.BalanceDate;
    public DateTime? LastSync { get; init; } = lunchFlowAccount.LastSync;
    public Guid? LinkedAccountId { get; init; } = lunchFlowAccount.LinkedAccountId;
}

public interface ILunchFlowAccountUpdateRequest
{
    Guid ID { get; }
    string Name { get; }
    string InstitutionName { get; }
    string InstitutionLogo { get; }
    string Provider { get; }
    string? Currency { get; }
    string? Status { get; }
    decimal Balance { get; }
    DateTime BalanceDate { get; }
    DateTime? LastSync { get; }
}

public class LunchFlowAccountUpdateRequest : ILunchFlowAccountUpdateRequest
{
    public Guid ID { get; init; }
    public string Name { get; init; } = string.Empty;
    public string InstitutionName { get; init; } = string.Empty;
    public string InstitutionLogo { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Currency { get; init; } = string.Empty;
    public string? Status { get; init; } = string.Empty;
    public decimal Balance { get; init; } = 0;
    public DateTime BalanceDate { get; init; } = DateTime.UnixEpoch;
    public DateTime? LastSync { get; init; } = null;
}

public interface ILunchFlowAccountDeleteRequest
{
    public Guid ID { get; }
}

public class LunchFlowAccountDeleteRequest : ILunchFlowAccountDeleteRequest
{
    public Guid ID { get; init; }
}
