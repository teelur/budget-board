using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IInstitutionCreateRequest
{
    string Name { get; }
}

public class InstitutionCreateRequest : IInstitutionCreateRequest
{
    public string Name { get; set; } = string.Empty;
}

public interface IInstitutionUpdateRequest
{
    Guid ID { get; }
    string Name { get; }
}

public class InstitutionUpdateRequest : IInstitutionUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

public interface IInstitutionIndexRequest
{
    Guid ID { get; }
    int Index { get; }
}

public class InstitutionIndexRequest : IInstitutionIndexRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public int Index { get; set; } = 0;
}

public interface IInstitutionResponse
{
    Guid ID { get; }
    string Name { get; }
    DateTime? Deleted { get; }
    int Index { get; }
    Guid UserID { get; }
    IEnumerable<IAccountResponse> Accounts { get; }
}

public class InstitutionResponse : IInstitutionResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Index { get; set; } = 0;
    public DateTime? Deleted { get; set; } = null;
    public Guid UserID { get; set; } = Guid.NewGuid();
    public IEnumerable<IAccountResponse> Accounts { get; set; } = [];

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
