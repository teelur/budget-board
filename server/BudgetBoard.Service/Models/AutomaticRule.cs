namespace BudgetBoard.Service.Models;

public interface IRuleParameterRequest
{
    string Field { get; }
    string Operator { get; }
    string Value { get; }
    string Type { get; }
}

public class RuleParameterCreateRequest() : IRuleParameterRequest
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAutomaticRuleCreateRequest
{
    ICollection<RuleParameterCreateRequest> Conditions { get; }
    ICollection<RuleParameterCreateRequest> Actions { get; }
}

public class AutomaticRuleCreateRequest() : IAutomaticRuleCreateRequest
{
    public ICollection<RuleParameterCreateRequest> Conditions { get; set; } = [];
    public ICollection<RuleParameterCreateRequest> Actions { get; set; } = [];
}

public interface IRuleParameterResponse : IRuleParameterRequest
{
    Guid ID { get; }
}

public class RuleParameterResponse() : IRuleParameterResponse
{
    public required Guid ID { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAutomaticRuleResponse
{
    Guid ID { get; }
    ICollection<RuleParameterResponse> Conditions { get; }
    ICollection<RuleParameterResponse> Actions { get; }
}

public class AutomaticRuleResponse() : IAutomaticRuleResponse
{
    public Guid ID { get; set; }
    public ICollection<RuleParameterResponse> Conditions { get; set; } = [];
    public ICollection<RuleParameterResponse> Actions { get; set; } = [];
}

public class RuleParameterUpdateRequest() : IRuleParameterRequest
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public interface IAutomaticRuleUpdateRequest
{
    Guid ID { get; }
    ICollection<IRuleParameterRequest> Conditions { get; }
    ICollection<IRuleParameterRequest> Actions { get; }
}

public class AutomaticRuleUpdateRequest() : IAutomaticRuleUpdateRequest
{
    public required Guid ID { get; set; }
    public ICollection<IRuleParameterRequest> Conditions { get; set; } = [];
    public ICollection<IRuleParameterRequest> Actions { get; set; } = [];
}
