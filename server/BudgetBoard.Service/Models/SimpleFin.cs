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

public interface ISimpleFinOrganization
{
    string? Domain { get; }

    [JsonPropertyName("sfin-url")]
    string SimpleFinUrl { get; }
    string? Name { get; }
}

public class SimpleFinOrganization : ISimpleFinOrganization
{
    public string? Domain { get; init; }
    public string SimpleFinUrl { get; init; } = string.Empty;
    public string? Name { get; init; }
}

public interface ISimpleFinTransaction
{
    string Id { get; }
    int Posted { get; }
    string Amount { get; }
    string Description { get; }

    [JsonPropertyName("transacted_at")]
    int TransactedAt { get; }
    bool Pending { get; }
}

public class SimpleFinTransaction : ISimpleFinTransaction
{
    public string Id { get; init; } = string.Empty;
    public int Posted { get; init; }
    public string Amount { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int TransactedAt { get; init; }
    public bool Pending { get; init; }
}

public interface ISimpleFinAccount
{
    ISimpleFinOrganization Org { get; }
    string Id { get; }
    string Name { get; }
    string Currency { get; }
    string Balance { get; }

    [JsonPropertyName("available-balance")]
    string? AvailableBalance { get; }

    [JsonPropertyName("balance-date")]
    int BalanceDate { get; }
    IEnumerable<ISimpleFinTransaction> Transactions { get; }
}

public class SimpleFinAccount : ISimpleFinAccount
{
    public SimpleFinOrganization Org { get; init; } = new();
    ISimpleFinOrganization ISimpleFinAccount.Org => Org;
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string Balance { get; init; } = string.Empty;
    public string? AvailableBalance { get; init; }
    public int BalanceDate { get; init; }
    public IEnumerable<SimpleFinTransaction> Transactions { get; init; } = [];
    IEnumerable<ISimpleFinTransaction> ISimpleFinAccount.Transactions => Transactions;
}

public interface ISimpleFinAccountData
{
    IEnumerable<string> Errors { get; }
    IEnumerable<ISimpleFinAccount> Accounts { get; }
}

public class SimpleFinAccountData : ISimpleFinAccountData
{
    public IEnumerable<string> Errors { get; init; } = [];
    public IEnumerable<SimpleFinAccount> Accounts { get; init; } = [];
    IEnumerable<ISimpleFinAccount> ISimpleFinAccountData.Accounts => Accounts;
}
