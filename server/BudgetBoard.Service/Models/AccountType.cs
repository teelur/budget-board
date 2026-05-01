using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IAccountType
{
    public string Value { get; }
    public string Parent { get; }
}

public class AccountTypeBase : IAccountType
{
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AccountTypeBase()
    {
        Value = string.Empty;
        Parent = string.Empty;
    }
}

public interface IAccountTypeCreateRequest : IAccountType { }

public class AccountTypeCreateRequest : IAccountTypeCreateRequest
{
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AccountTypeCreateRequest()
    {
        Value = string.Empty;
        Parent = string.Empty;
    }
}

public interface IAccountTypeUpdateRequest
{
    Guid ID { get; }
    string Value { get; }
    string Parent { get; }
}

public class AccountTypeUpdateRequest : IAccountTypeUpdateRequest
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }

    [JsonConstructor]
    public AccountTypeUpdateRequest()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
    }
}

public interface IAccountTypeResponse
{
    Guid ID { get; }
    string Value { get; }
    string Parent { get; }
    Guid UserID { get; }
}

public class AccountTypeResponse : IAccountTypeResponse
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public AccountTypeResponse()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
        UserID = Guid.Empty;
    }

    public AccountTypeResponse(AccountType accountType)
    {
        ID = accountType.ID;
        Value = accountType.Value;
        Parent = accountType.Parent;
        UserID = accountType.UserID;
    }
}
