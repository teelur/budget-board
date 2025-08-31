using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IAutomaticCategorizationRuleService
{
    Task CreateAutomaticCategorizationRuleAsync(
        Guid userGuid,
        AutomaticCategorizationRuleCreateRequest rule
    );
    Task<IEnumerable<IAutomaticCategorizationRuleResponse>> ReadAutomaticCategorizationRulesAsync(
        Guid userGuid
    );
    Task UpdateAutomaticCategorizationRuleAsync(
        Guid userGuid,
        AutomaticCategorizationRuleUpdateRequest rule
    );
    Task DeleteAutomaticCategorizationRuleAsync(Guid userGuid, Guid ruleGuid);
}
