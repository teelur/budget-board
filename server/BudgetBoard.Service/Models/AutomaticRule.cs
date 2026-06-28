using BudgetBoard.Database.Models;

namespace BudgetBoard.Service.Models;

public interface IRuleParameterRequest
{
    string Field { get; }
    string Operator { get; }
    string Value { get; }
}

public class RuleParameterCreateRequest : IRuleParameterRequest
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }

    public RuleParameterCreateRequest()
    {
        Field = string.Empty;
        Operator = string.Empty;
        Value = string.Empty;
    }

    public RuleParameterCreateRequest(IRuleParameterResponse condition)
    {
        Field = condition.Field;
        Operator = condition.Operator;
        Value = condition.Value;
    }
}

public interface IAutomaticRuleCreateRequest
{
    ICollection<RuleParameterCreateRequest> Conditions { get; }
    ICollection<RuleParameterCreateRequest> Actions { get; }
}

public class AutomaticRuleCreateRequest : IAutomaticRuleCreateRequest
{
    public ICollection<RuleParameterCreateRequest> Conditions { get; set; }
    public ICollection<RuleParameterCreateRequest> Actions { get; set; }

    public AutomaticRuleCreateRequest()
    {
        Conditions = [];
        Actions = [];
    }

    public AutomaticRuleCreateRequest(IAutomaticRuleResponse rule)
    {
        Conditions = [.. rule.Conditions.Select(c => new RuleParameterCreateRequest(c))];
        Actions = [.. rule.Actions.Select(a => new RuleParameterCreateRequest(a))];
    }
}

public interface IRuleParameterResponse : IRuleParameterRequest
{
    Guid ID { get; }
}

public class RuleParameterResponse : IRuleParameterResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public RuleParameterResponse(RuleCondition condition)
    {
        ID = condition.ID;
        Field = condition.Field;
        Operator = condition.Operator;
        Value = condition.Value;
    }

    public RuleParameterResponse(RuleAction action)
    {
        ID = action.ID;
        Field = action.Field;
        Operator = action.Operator;
        Value = action.Value;
    }
}

public interface IAutomaticRuleResponse
{
    Guid ID { get; }
    ICollection<RuleParameterResponse> Conditions { get; }
    ICollection<RuleParameterResponse> Actions { get; }
}

public class AutomaticRuleResponse : IAutomaticRuleResponse
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public ICollection<RuleParameterResponse> Conditions { get; set; } = [];
    public ICollection<RuleParameterResponse> Actions { get; set; } = [];

    public AutomaticRuleResponse(AutomaticRule rule)
    {
        ID = rule.ID;
        Conditions = [.. rule.Conditions.Select(c => new RuleParameterResponse(c))];
        Actions = [.. rule.Actions.Select(a => new RuleParameterResponse(a))];
    }
}

public class RuleParameterUpdateRequest() : IRuleParameterRequest
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public interface IAutomaticRuleUpdateRequest
{
    Guid ID { get; }
    ICollection<RuleParameterUpdateRequest> Conditions { get; }
    ICollection<RuleParameterUpdateRequest> Actions { get; }
}

public class AutomaticRuleUpdateRequest() : IAutomaticRuleUpdateRequest
{
    public required Guid ID { get; set; }
    public ICollection<RuleParameterUpdateRequest> Conditions { get; set; } = [];
    public ICollection<RuleParameterUpdateRequest> Actions { get; set; } = [];
}
