using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IAutomaticRuleService
{
    Task CreateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest rule);
    Task<IEnumerable<IAutomaticRuleResponse>> ReadAutomaticRulesAsync(Guid userGuid);
    Task UpdateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleUpdateRequest rule);
    Task DeleteAutomaticRuleAsync(Guid userGuid, Guid ruleGuid);
    Task<string> RunAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest rule);
}
