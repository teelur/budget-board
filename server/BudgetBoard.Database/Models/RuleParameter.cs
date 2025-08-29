namespace BudgetBoard.Database.Models;

public abstract class RuleParameterBase
{
    public Guid ID { get; set; }

    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public Guid RuleID { get; set; }
    public AutomaticCategorizationRule Rule { get; set; } = null!;
}

// Derived entity for conditions
public class RuleCondition : RuleParameterBase { }

// Derived entity for actions
public class RuleAction : RuleParameterBase { }
