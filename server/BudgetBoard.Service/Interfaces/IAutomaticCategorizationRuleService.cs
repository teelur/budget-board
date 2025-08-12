using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IAutomaticCategorizationRuleService
{
    Task CreateAutomaticCategorizationRuleAsync(
        Guid userGuid,
        IAutomaticCategorizationRuleRequest rule
    );
    Task<IEnumerable<IAutomaticCategorizationRuleResponse>> ReadAutomaticCategorizationRulesAsync(
        Guid userGuid
    );
    Task DeleteAutomaticCategorizationRuleAsync(Guid userGuid, Guid ruleGuid);
}
