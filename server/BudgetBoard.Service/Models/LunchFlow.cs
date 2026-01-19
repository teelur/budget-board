using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface ILunchFlowAccountsData
{
    IEnumerable<ILunchFlowAccountData> Accounts { get; }
    int Total { get; }
}

public class LunchFlowAccountsData : ILunchFlowAccountsData
{
    public IEnumerable<ILunchFlowAccountData> Accounts { get; init; } = [];
    public int Total { get; init; }
}

public interface ILunchFlowAccountData
{
    string ID { get; }
    string Name { get; }

    [JsonPropertyName("institution_name")]
    string InstitutionName { get; }

    [JsonPropertyName("institution_logo")]
    string InstitutionLogo { get; }
    string Provider { get; }
    string? Currency { get; }
    string? Status { get; }
}

public class LunchFlowAccountData : ILunchFlowAccountData
{
    public string ID { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("institution_name")]
    public string InstitutionName { get; init; } = string.Empty;

    [JsonPropertyName("institution_logo")]
    public string InstitutionLogo { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Currency { get; init; } = null;
    public string? Status { get; init; } = null;
}

public interface ILunchFlowTransactionData
{
    string ID { get; }
    string AccountID { get; }
    int Amount { get; }
    string Currency { get; }
    string Date { get; }
    string? Merchant { get; }
    string? Description { get; }
    bool? IsPending { get; }
}

public class LunchFlowTransactionData : ILunchFlowTransactionData
{
    public string ID { get; init; } = string.Empty;
    public string AccountID { get; init; } = string.Empty;
    public int Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string? Merchant { get; init; } = null;
    public string? Description { get; init; } = null;
    public bool? IsPending { get; init; } = null;
}

public interface ILunchFlowTransactionsData
{
    IEnumerable<ILunchFlowTransactionData> Transactions { get; }
    int Total { get; }
}

public class LunchFlowTransactionsData : ILunchFlowTransactionsData
{
    public IEnumerable<ILunchFlowTransactionData> Transactions { get; init; } = [];
    public int Total { get; init; }
}

public interface ILunchFlowBalanceData
{
    int Balance { get; }
    string Currency { get; }
}

public class LunchFlowBalanceData : ILunchFlowBalanceData
{
    public int Balance { get; init; } = 0;
    public string Currency { get; init; } = string.Empty;
}
