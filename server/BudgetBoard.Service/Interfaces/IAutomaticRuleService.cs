using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IAutomaticRuleService
{
    Task CreateAutomaticRuleAsync(Guid userGuid, AutomaticRuleCreateRequest rule);
    Task<IEnumerable<IAutomaticRuleResponse>> ReadAutomaticRulesAsync(Guid userGuid);
    Task UpdateAutomaticRuleAsync(Guid userGuid, AutomaticRuleUpdateRequest rule);
    Task DeleteAutomaticRuleAsync(Guid userGuid, Guid ruleGuid);
}
