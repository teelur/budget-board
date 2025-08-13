namespace BudgetBoard.Service.Models;

public interface IAutomaticCategorizationRuleRequest
{
    string CategorizationRule { get; set; }
    string Category { get; set; }
}

public class AutomaticCategorizationRuleRequest() : IAutomaticCategorizationRuleRequest
{
    public string CategorizationRule { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public interface IAutomaticCategorizationRuleResponse
{
    Guid ID { get; set; }
    string CategorizationRule { get; set; }
    string Category { get; set; }
}

public class AutomaticCategorizationRuleResponse() : IAutomaticCategorizationRuleResponse
{
    public Guid ID { get; set; }
    public string CategorizationRule { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
