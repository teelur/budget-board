using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IInstitutionCreateRequest
{
    string Name { get; }
}

public class InstitutionCreateRequest : IInstitutionCreateRequest
{
    public string Name { get; set; }

    [JsonConstructor]
    public InstitutionCreateRequest()
    {
        Name = string.Empty;
    }
}

public interface IInstitutionUpdateRequest
{
    Guid ID { get; }
    string Name { get; }
}

public class InstitutionUpdateRequest : IInstitutionUpdateRequest
{
    public Guid ID { get; set; }
    public string Name { get; set; }

    [JsonConstructor]
    public InstitutionUpdateRequest()
    {
        ID = Guid.NewGuid();
        Name = string.Empty;
    }
}

public interface IInstitutionIndexRequest
{
    Guid ID { get; }
    int Index { get; }
}

public class InstitutionIndexRequest : IInstitutionIndexRequest
{
    public Guid ID { get; set; }
    public int Index { get; set; }
}

public interface IInstitutionResponse
{
    Guid ID { get; }
    string Name { get; }
    int Index { get; }
    Guid UserID { get; }
    IEnumerable<IAccountResponse> Accounts { get; }
}

public class InstitutionResponse : IInstitutionResponse
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public int Index { get; set; }
    public DateTime? Deleted { get; set; }
    public Guid UserID { get; set; }
    public IEnumerable<IAccountResponse> Accounts { get; set; }

    [JsonConstructor]
    public InstitutionResponse()
    {
        ID = Guid.NewGuid();
        Name = string.Empty;
        Index = 0;
        Deleted = null;
        UserID = Guid.NewGuid();
        Accounts = [];
    }

    public InstitutionResponse(Institution institution)
    {
        ID = institution.ID;
        Name = institution.Name;
        Index = institution.Index;
        Deleted = institution.Deleted;
        UserID = institution.UserID;
        Accounts = institution.Accounts.Select(a => new AccountResponse(a));
    }
}
