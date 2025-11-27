using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface ISimpleFinData
{
    string Auth { get; }
    string BaseUrl { get; }
}

public class SimpleFinData() : ISimpleFinData
{
    public string Auth { get; } = string.Empty;
    public string BaseUrl { get; } = string.Empty;

    public SimpleFinData(string auth, string baseUrl)
        : this()
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

public class SimpleFinOrganization() : ISimpleFinOrganization
{
    public string? Domain { get; set; }
    public string SimpleFinUrl { get; set; } = string.Empty;
    public string? Name { get; set; }
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

public class SimpleFinTransaction() : ISimpleFinTransaction
{
    public string Id { get; set; } = string.Empty;
    public int Posted { get; set; }
    public string Amount { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TransactedAt { get; set; }
    public bool Pending { get; set; }
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
    public ISimpleFinOrganization Org { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string Currency { get; set; }
    public string Balance { get; set; }
    public string? AvailableBalance { get; set; }
    public int BalanceDate { get; set; }
    public IEnumerable<ISimpleFinTransaction> Transactions { get; set; }

    [JsonConstructor]
    public SimpleFinAccount()
    {
        Org = new SimpleFinOrganization();
        Id = string.Empty;
        Name = string.Empty;
        Currency = string.Empty;
        Balance = string.Empty;
        AvailableBalance = string.Empty;
        BalanceDate = 0;
        Transactions = [];
    }
}

public interface ISimpleFinAccountData
{
    IEnumerable<string> Errors { get; }
    IEnumerable<ISimpleFinAccount> Accounts { get; }
}

public class SimpleFinAccountData : ISimpleFinAccountData
{
    public IEnumerable<string> Errors { get; set; }
    public IEnumerable<ISimpleFinAccount> Accounts { get; set; }

    [JsonConstructor]
    public SimpleFinAccountData()
    {
        Errors = [];
        Accounts = [];
    }
}
