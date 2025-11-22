using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing automatic rules that apply actions to transactions based on conditions.
/// </summary>
public interface IAutomaticRuleService
{
    /// <summary>
    /// Creates a new automatic rule for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="rule">The automatic rule creation details.</param>
    Task CreateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest rule);

    /// <summary>
    /// Retrieves automatic rules for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A collection of automatic rule details.</returns>
    Task<IReadOnlyList<IAutomaticRuleResponse>> ReadAutomaticRulesAsync(Guid userGuid);

    /// <summary>
    /// Updates an existing automatic rule for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="rule">The automatic rule update details.</param>
    Task UpdateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleUpdateRequest rule);

    /// <summary>
    /// Deletes an automatic rule for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="ruleGuid">The unique identifier of the rule to delete.</param>
    Task DeleteAutomaticRuleAsync(Guid userGuid, Guid ruleGuid);

    /// <summary>
    /// Runs an automatic rule immediately against existing transactions.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="rule">The automatic rule details to run.</param>
    /// <returns>A summary string describing the result of the rule execution.</returns>
    Task<string> RunAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest rule);
}
