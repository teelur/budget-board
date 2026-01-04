using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface ISimpleFinData
{
    string Auth { get; }
    string BaseUrl { get; }
}

public class SimpleFinData : ISimpleFinData
{
    public string Auth { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;

    public SimpleFinData() { }

    public SimpleFinData(string auth, string baseUrl)
    {
        Auth = auth;
        BaseUrl = baseUrl;
    }
}

public interface ISimpleFinOrganizationData
{
    string? Domain { get; }
    string SimpleFinUrl { get; }
    string? Name { get; }
    string? Url { get; }
    string? SyncID { get; }
}

public class SimpleFinOrganizationData : ISimpleFinOrganizationData
{
    public string? Domain { get; init; } = null;

    [JsonPropertyName("sfin-url")]
    public string SimpleFinUrl { get; init; } = string.Empty;
    public string? Name { get; init; } = null;
    public string? Url { get; init; } = null;

    [JsonPropertyName("id")]
    public string? SyncID { get; init; } = null;
}

public interface ISimpleFinTransactionData
{
    string Id { get; }
    int Posted { get; }
    string Amount { get; }
    string Description { get; }
    int TransactedAt { get; }
    bool Pending { get; }
}

public class SimpleFinTransactionData : ISimpleFinTransactionData
{
    public string Id { get; init; } = string.Empty;
    public int Posted { get; init; }
    public string Amount { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("transacted_at")]
    public int TransactedAt { get; init; }
    public bool Pending { get; init; }
}

public interface ISimpleFinAccountData
{
    ISimpleFinOrganizationData Org { get; }
    string Id { get; }
    string Name { get; }
    string Currency { get; }
    string Balance { get; }
    string? AvailableBalance { get; }
    int BalanceDate { get; }
    IEnumerable<ISimpleFinTransactionData> Transactions { get; }
}

public class SimpleFinAccountData : ISimpleFinAccountData
{
    public SimpleFinOrganizationData Org { get; init; } = new();
    ISimpleFinOrganizationData ISimpleFinAccountData.Org => Org;
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string Balance { get; init; } = string.Empty;

    [JsonPropertyName("available-balance")]
    public string? AvailableBalance { get; init; }

    [JsonPropertyName("balance-date")]
    public int BalanceDate { get; init; }
    public IEnumerable<SimpleFinTransactionData> Transactions { get; init; } = [];
    IEnumerable<ISimpleFinTransactionData> ISimpleFinAccountData.Transactions => Transactions;
}

public interface ISimpleFinAccountsData
{
    IEnumerable<string> Errors { get; }
    IEnumerable<ISimpleFinAccountData> Accounts { get; }
}

public class SimpleFinAccountsData : ISimpleFinAccountsData
{
    public IEnumerable<string> Errors { get; init; } = [];
    public IEnumerable<SimpleFinAccountData> Accounts { get; init; } = [];
    IEnumerable<ISimpleFinAccountData> ISimpleFinAccountsData.Accounts => Accounts;
}
