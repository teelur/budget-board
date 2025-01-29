﻿using BudgetBoard.Database.Models;
using BudgetBoard.Service.Types;
using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Models;

public interface IInstitutionCreateRequest
{
    string Name { get; set; }
    int Index { get; set; }
    Guid UserID { get; set; }
}
public class InstitutionCreateRequest : IInstitutionCreateRequest
{
    public string Name { get; set; }
    public int Index { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public InstitutionCreateRequest()
    {
        Name = string.Empty;
        Index = 0;
        UserID = Guid.NewGuid();

    }
}

public interface IInstitutionUpdateRequest
{
    Guid ID { get; set; }
    string Name { get; set; }
    int Index { get; set; }
    Guid UserID { get; set; }
}
public class InstitutionUpdateRequest : IInstitutionUpdateRequest
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public int Index { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public InstitutionUpdateRequest()
    {
        ID = Guid.NewGuid();
        Name = string.Empty;
        Index = 0;
        UserID = Guid.NewGuid();
    }
}

public interface IInstitutionIndexRequest
{
    Guid ID { get; set; }
    int Index { get; set; }
}
public class InstitutionIndexRequest : IInstitutionIndexRequest
{
    public Guid ID { get; set; }
    public int Index { get; set; }
}

public interface IInstitutionResponse
{
    Guid ID { get; set; }
    string Name { get; set; }
    int Index { get; set; }
    Guid UserID { get; set; }
    IEnumerable<IAccountResponse> Accounts { get; set; }
}
public class InstitutionResponse : IInstitutionResponse
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public int Index { get; set; }
    public Guid UserID { get; set; }
    public IEnumerable<IAccountResponse> Accounts { get; set; }

    [JsonConstructor]
    public InstitutionResponse()
    {
        ID = Guid.NewGuid();
        Name = string.Empty;
        Index = 0;
        UserID = Guid.NewGuid();
        Accounts = [];
    }

    public InstitutionResponse(Institution institution)
    {
        ID = institution.ID;
        Name = institution.Name;
        Index = institution.Index;
        UserID = institution.UserID;
        Accounts = institution.Accounts.Select(a => new AccountResponse(a));
    }
}
