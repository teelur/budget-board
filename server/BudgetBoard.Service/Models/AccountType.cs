using System.Text.Json.Serialization;
using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IAccountType
{
    public string Value { get; }
    public string Parent { get; }
    public string Classification { get; }
}

public class AccountTypeBase : IAccountType
{
    public string Value { get; set; }
    public string Parent { get; set; }
    public string Classification { get; set; }

    [JsonConstructor]
    public AccountTypeBase()
    {
        Value = string.Empty;
        Parent = string.Empty;
        Classification = AccountTypeClassification.Asset;
    }
}

public interface IAccountTypeCreateRequest : IAccountType { }

public class AccountTypeCreateRequest : IAccountTypeCreateRequest
{
    public string Value { get; set; }
    public string Parent { get; set; }
    public string Classification { get; set; }

    [JsonConstructor]
    public AccountTypeCreateRequest()
    {
        Value = string.Empty;
        Parent = string.Empty;
        Classification = AccountTypeClassification.Asset;
    }
}

public interface IAccountTypeUpdateRequest
{
    Guid ID { get; }
    string Value { get; }
    string Parent { get; }
    string Classification { get; }
}

public class AccountTypeUpdateRequest : IAccountTypeUpdateRequest
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }
    public string Classification { get; set; }

    [JsonConstructor]
    public AccountTypeUpdateRequest()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
        Classification = AccountTypeClassification.Asset;
    }
}

public interface IAccountTypeResponse
{
    Guid ID { get; }
    string Value { get; }
    string Parent { get; }
    string Classification { get; }
}

public class AccountTypeResponse : IAccountTypeResponse
{
    public Guid ID { get; set; }
    public string Value { get; set; }
    public string Parent { get; set; }
    public string Classification { get; set; }

    [JsonConstructor]
    public AccountTypeResponse()
    {
        ID = Guid.Empty;
        Value = string.Empty;
        Parent = string.Empty;
        Classification = AccountTypeClassification.Asset;
    }

    public AccountTypeResponse(AccountType accountType)
    {
        ID = accountType.ID;
        Value = accountType.Value;
        Parent = accountType.Parent;
        Classification = accountType.Classification;
    }

    public AccountTypeResponse(IAccountType accountType)
    {
        ID = Guid.Empty;
        Value = accountType.Value;
        Parent = accountType.Parent;
        Classification = accountType.Classification;
    }
}
