namespace BudgetBoard.Service.Models;

public interface ISimpleFinOrganizationCreateRequest
{
    string? Domain { get; }
    string SimpleFinUrl { get; }
    string? Name { get; }
    string? Url { get; }
    string? SyncID { get; }
}

public class SimpleFinOrganizationCreateRequest : ISimpleFinOrganizationCreateRequest
{
    public string? Domain { get; init; } = null;
    public string SimpleFinUrl { get; init; } = string.Empty;
    public string? Name { get; init; } = null;
    public string? Url { get; init; } = null;
    public string? SyncID { get; init; } = null;
}

public interface ISimpleFinOrganizationResponse
{
    public Guid ID { get; }
    public string? Domain { get; }
    public string SimpleFinUrl { get; }
    public string? Name { get; }
    public string? Url { get; }
    public string? SyncID { get; }
    IEnumerable<ISimpleFinAccountResponse> Accounts { get; }
}

public class SimpleFinOrganizationResponse(
    Database.Models.SimpleFinOrganization simpleFinOrganization
) : ISimpleFinOrganizationResponse
{
    public Guid ID { get; init; } = simpleFinOrganization.ID;
    public string? Domain { get; init; } = simpleFinOrganization.Domain;
    public string SimpleFinUrl { get; init; } = simpleFinOrganization.SimpleFinUrl;
    public string? Name { get; init; } = simpleFinOrganization.Name;
    public string? Url { get; init; } = simpleFinOrganization.Url;
    public string? SyncID { get; init; } = simpleFinOrganization.SyncID;
    public IEnumerable<ISimpleFinAccountResponse> Accounts { get; init; } =
        simpleFinOrganization.Accounts.Select(a => new SimpleFinAccountResponse(a)).ToList();
}

public interface ISimpleFinOrganizationUpdateRequest
{
    public Guid ID { get; }
    public string? Domain { get; }
    public string SimpleFinUrl { get; }
    public string? Name { get; }
    public string? Url { get; }
    public string? SyncID { get; }
}

public class SimpleFinOrganizationUpdateRequest : ISimpleFinOrganizationUpdateRequest
{
    public Guid ID { get; init; } = Guid.NewGuid();
    public string? Domain { get; init; } = null;
    public string SimpleFinUrl { get; init; } = string.Empty;
    public string? Name { get; init; } = null;
    public string? Url { get; init; } = null;
    public string? SyncID { get; init; } = null;
}
