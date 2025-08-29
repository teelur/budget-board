namespace BudgetBoard.Service.Models;

public interface IRuleParameterCreateRequest
{
    string Field { get; }
    string Operator { get; }
    string Value { get; }
    string Type { get; }
}

public class RuleParameterCreateRequest() : IRuleParameterCreateRequest
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAutomaticCategorizationRuleCreateRequest
{
    ICollection<IRuleParameterCreateRequest> Conditions { get; }
    ICollection<IRuleParameterCreateRequest> Actions { get; }
}

public class AutomaticCategorizationRuleCreateRequest() : IAutomaticCategorizationRuleCreateRequest
{
    public ICollection<IRuleParameterCreateRequest> Conditions { get; set; } = [];
    public ICollection<IRuleParameterCreateRequest> Actions { get; set; } = [];
}

public interface IRuleParameterResponse
{
    Guid ID { get; }
    string Field { get; }
    string Operator { get; }
    string Value { get; }
    string Type { get; }
}

public class RuleParameterResponse() : IRuleParameterResponse
{
    public required Guid ID { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAutomaticCategorizationRuleResponse
{
    Guid ID { get; }
    ICollection<IRuleParameterResponse> Conditions { get; }
    ICollection<IRuleParameterResponse> Actions { get; }
}

public class AutomaticCategorizationRuleResponse() : IAutomaticCategorizationRuleResponse
{
    public Guid ID { get; set; }
    public ICollection<IRuleParameterResponse> Conditions { get; set; } = [];
    public ICollection<IRuleParameterResponse> Actions { get; set; } = [];
}

public interface IRuleParameterUpdateRequest
{
    Guid ID { get; }
    string Field { get; }
    string Operator { get; }
    string Value { get; }
    string Type { get; }
}

public class RuleParameterUpdateRequest() : IRuleParameterUpdateRequest
{
    public required Guid ID { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAutomaticCategorizationRuleUpdateRequest
{
    Guid ID { get; }
    ICollection<IRuleParameterUpdateRequest> Conditions { get; }
    ICollection<IRuleParameterUpdateRequest> Actions { get; }
}

public class AutomaticCategorizationRuleUpdateRequest() : IAutomaticCategorizationRuleUpdateRequest
{
    public required Guid ID { get; set; }
    public ICollection<IRuleParameterUpdateRequest> Conditions { get; set; } = [];
    public ICollection<IRuleParameterUpdateRequest> Actions { get; set; } = [];
}
