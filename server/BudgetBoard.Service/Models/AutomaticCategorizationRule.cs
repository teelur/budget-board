namespace BudgetBoard.Service.Models;

public class RuleParameterCreateRequest()
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class AutomaticCategorizationRuleCreateRequest()
{
    public ICollection<RuleParameterCreateRequest> Conditions { get; set; } = [];
    public ICollection<RuleParameterCreateRequest> Actions { get; set; } = [];
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

public class RuleParameterUpdateRequest()
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class AutomaticCategorizationRuleUpdateRequest()
{
    public required Guid ID { get; set; }
    public ICollection<RuleParameterUpdateRequest> Conditions { get; set; } = [];
    public ICollection<RuleParameterUpdateRequest> Actions { get; set; } = [];
}
