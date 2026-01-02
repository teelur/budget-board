namespace BudgetBoard.Service.Models;

public interface ISimpleFinAccountCreateRequest
{
    string SyncID { get; }
    string Name { get; }
    string Currency { get; }
    decimal Balance { get; }
    int BalanceDate { get; }
    Guid OrganizationId { get; }
}

public class SimpleFinAccountCreateRequest : ISimpleFinAccountCreateRequest
{
    public string SyncID { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal Balance { get; init; } = 0;
    public int BalanceDate { get; init; } = 0;
    public Guid OrganizationId { get; init; } = Guid.Empty;
}

public interface ISimpleFinAccountResponse
{
    public Guid ID { get; }
    public string SyncID { get; }
    public string Name { get; }
    public string Currency { get; }
    public decimal Balance { get; }
    public DateTime BalanceDate { get; }
    public DateTime? LastSync { get; }
    public Guid? OrganizationId { get; }
    public Guid? LinkedAccountId { get; }
}

public class SimpleFinAccountResponse(Database.Models.SimpleFinAccount simpleFinAccount)
    : ISimpleFinAccountResponse
{
    public Guid ID { get; init; } = simpleFinAccount.ID;
    public string SyncID { get; init; } = simpleFinAccount.SyncID;
    public string Name { get; init; } = simpleFinAccount.Name;
    public string Currency { get; init; } = simpleFinAccount.Currency;
    public decimal Balance { get; init; } = simpleFinAccount.Balance;
    public DateTime BalanceDate { get; init; } =
        DateTimeOffset.FromUnixTimeSeconds(simpleFinAccount.BalanceDate).UtcDateTime;
    public DateTime? LastSync { get; init; } = simpleFinAccount.LastSync;
    public Guid? OrganizationId { get; init; } = simpleFinAccount.OrganizationId;
    public Guid? LinkedAccountId { get; init; } = simpleFinAccount.LinkedAccountId;
}

public interface ISimpleFinAccountUpdateRequest
{
    public Guid ID { get; }
    public string SyncID { get; }
    public string Name { get; }
    public string Currency { get; }
    public decimal Balance { get; }
    public DateTime BalanceDate { get; }
    public DateTime? LastSync { get; }
}

public class SimpleFinAccountUpdateRequest : ISimpleFinAccountUpdateRequest
{
    public Guid ID { get; init; }
    public string SyncID { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal Balance { get; init; } = 0.0M;
    public DateTime BalanceDate { get; init; } = DateTime.UtcNow;
    public DateTime? LastSync { get; init; } = null;
    public Guid? OrganizationId { get; init; } = null;
    public Guid? LinkedAccountId { get; init; } = null;
}
