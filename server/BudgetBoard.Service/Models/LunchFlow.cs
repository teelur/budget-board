using System.Text.Json.Serialization;
using BudgetBoard.Service.Helpers;

namespace BudgetBoard.Service.Models;

public interface ILunchFlowAccountsData
{
    IEnumerable<ILunchFlowAccountData> Accounts { get; }
    int Total { get; }
}

public class LunchFlowAccountsData : ILunchFlowAccountsData
{
    public IEnumerable<LunchFlowAccountData> Accounts { get; init; } = [];
    IEnumerable<ILunchFlowAccountData> ILunchFlowAccountsData.Accounts => Accounts;
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
    [JsonConverter(typeof(NumberToStringConverter))]
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
    decimal Amount { get; }
    string Currency { get; }
    string Date { get; }
    string? Merchant { get; }
    string? Description { get; }
    bool? IsPending { get; }
}

public class LunchFlowTransactionData : ILunchFlowTransactionData
{
    [JsonConverter(typeof(NumberToStringConverter))]
    public string ID { get; init; } = string.Empty;

    [JsonConverter(typeof(NumberToStringConverter))]
    public string AccountID { get; init; } = string.Empty;
    public decimal Amount { get; init; }
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
    public IEnumerable<LunchFlowTransactionData> Transactions { get; init; } = [];
    IEnumerable<ILunchFlowTransactionData> ILunchFlowTransactionsData.Transactions => Transactions;
    public int Total { get; init; }
}

public interface ILunchFlowBalanceData
{
    decimal Amount { get; }
    string Currency { get; }
}

public class LunchFlowBalanceData : ILunchFlowBalanceData
{
    public decimal Amount { get; init; } = 0;
    public string Currency { get; init; } = string.Empty;
}

public interface ILunchFlowBalancesData
{
    ILunchFlowBalanceData? Balance { get; }
}

public class LunchFlowBalancesData : ILunchFlowBalancesData
{
    public LunchFlowBalanceData? Balance { get; init; }
    ILunchFlowBalanceData? ILunchFlowBalancesData.Balance => Balance;
}
