namespace BudgetBoard.Database.Models;

/// <summary>
/// Base class for rule parameters, used for both conditions and actions.
/// </summary>
public abstract class RuleParameterBase
{
    /// <summary>
    /// Unique identifier for the rule parameter.
    /// </summary>
    public Guid ID { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The field to evaluate or modify.
    /// </summary>
    public required string Field { get; set; }

    /// <summary>
    /// The operator to use for evaluation or modification.
    /// </summary>
    public required string Operator { get; set; }

    /// <summary>
    /// The value to compare or assign.
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Identifier for the associated rule.
    /// </summary>
    public required Guid RuleID { get; set; }

    /// <summary>
    /// Reference to the associated automatic rule.
    /// </summary>
    public AutomaticRule? Rule { get; set; } = null;
}

/// <summary>
/// Derived entity for rule conditions.
/// </summary>
public class RuleCondition : RuleParameterBase { }

/// <summary>
/// Derived entity for rule actions.
/// </summary>
public class RuleAction : RuleParameterBase { }
