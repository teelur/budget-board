using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class AutomaticCategorizationRuleService(
    ILogger<IAutomaticCategorizationRuleService> logger,
    UserDataContext userDataContext
) : IAutomaticCategorizationRuleService
{
    private readonly ILogger<IAutomaticCategorizationRuleService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateAutomaticCategorizationRuleAsync(
        Guid userGuid,
        AutomaticCategorizationRuleCreateRequest rule
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (rule.Conditions.Count == 0)
        {
            _logger.LogError(
                "Attempt to create an automatic categorization rule with no conditions."
            );
            throw new BudgetBoardServiceException(
                "At least one condition must be provided for the rule."
            );
        }

        if (rule.Actions.Count == 0)
        {
            _logger.LogError("Attempt to create an automatic categorization rule with no actions.");
            throw new BudgetBoardServiceException(
                "At least one action must be provided for the rule."
            );
        }

        var newRule = new AutomaticCategorizationRule
        {
            ID = Guid.NewGuid(),
            UserID = userData.Id,
            Conditions =
            [
                .. rule.Conditions.Select(c => new RuleCondition
                {
                    ID = Guid.NewGuid(),
                    Field = c.Field,
                    Operator = c.Operator,
                    Value = c.Value,
                    Type = c.Type,
                }),
            ],
            Actions =
            [
                .. rule.Actions.Select(a => new RuleAction
                {
                    ID = Guid.NewGuid(),
                    Field = a.Field,
                    Operator = a.Operator,
                    Value = a.Value,
                    Type = a.Type,
                }),
            ],
        };

        _userDataContext.AutomaticCategorizationRules.Add(newRule);
        try
        {
            await _userDataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while saving the automatic categorization rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while saving the automatic categorization rule."
            );
        }
    }

    public async Task<
        IEnumerable<IAutomaticCategorizationRuleResponse>
    > ReadAutomaticCategorizationRulesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        return userData
            .AutomaticCategorizationRules.Select(r => new AutomaticCategorizationRuleResponse
            {
                ID = r.ID,
                Conditions =
                [
                    .. r.Conditions.Select(c => new RuleParameterResponse
                    {
                        ID = c.ID,
                        Field = c.Field,
                        Operator = c.Operator,
                        Value = c.Value,
                        Type = c.Type,
                    }),
                ],
                Actions =
                [
                    .. r.Actions.Select(a => new RuleParameterResponse
                    {
                        ID = a.ID,
                        Field = a.Field,
                        Operator = a.Operator,
                        Value = a.Value,
                        Type = a.Type,
                    }),
                ],
            })
            .ToList();
    }

    public async Task UpdateAutomaticCategorizationRuleAsync(
        Guid userGuid,
        AutomaticCategorizationRuleUpdateRequest updatedRule
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var existingRule = userData.AutomaticCategorizationRules.FirstOrDefault(r =>
            r.ID == updatedRule.ID
        );

        if (existingRule == null)
        {
            _logger.LogError(
                "Attempt to update an automatic categorization rule that does not exist."
            );
            throw new BudgetBoardServiceException("Automatic categorization rule not found.");
        }

        if (updatedRule.Conditions.Count == 0)
        {
            _logger.LogError(
                "Attempt to update an automatic categorization rule with no conditions."
            );
            throw new BudgetBoardServiceException(
                "At least one condition must be provided for the rule."
            );
        }

        if (updatedRule.Actions.Count == 0)
        {
            _logger.LogError("Attempt to update an automatic categorization rule with no actions.");
            throw new BudgetBoardServiceException(
                "At least one action must be provided for the rule."
            );
        }

        existingRule.Conditions.Clear();
        foreach (var condition in updatedRule.Conditions)
        {
            existingRule.Conditions.Add(
                new RuleCondition
                {
                    Field = condition.Field,
                    Operator = condition.Operator,
                    Value = condition.Value,
                    Type = condition.Type,
                    RuleID = existingRule.ID,
                }
            );
        }

        existingRule.Actions.Clear();
        foreach (var action in updatedRule.Actions)
        {
            existingRule.Actions.Add(
                new RuleAction
                {
                    Field = action.Field,
                    Operator = action.Operator,
                    Value = action.Value,
                    Type = action.Type,
                    RuleID = existingRule.ID,
                }
            );
        }

        try
        {
            await _userDataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while updating the automatic categorization rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while updating the automatic categorization rule."
            );
        }
    }

    public async Task DeleteAutomaticCategorizationRuleAsync(Guid userGuid, Guid ruleGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var rule = userData.AutomaticCategorizationRules.FirstOrDefault(r => r.ID == ruleGuid);
        if (rule == null)
        {
            _logger.LogError(
                "Attempt to delete an automatic categorization rule that does not exist."
            );
            throw new BudgetBoardServiceException("Automatic categorization rule not found.");
        }

        userData.AutomaticCategorizationRules.Remove(rule);
        try
        {
            await _userDataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while deleting the automatic categorization rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while deleting the automatic categorization rule."
            );
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.AutomaticCategorizationRules)
                .ThenInclude(r => r.Conditions)
                .Include(u => u.AutomaticCategorizationRules)
                .ThenInclude(r => r.Actions)
                .Include(u => u.TransactionCategories)
                .AsSplitQuery()
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while retrieving the user data: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while retrieving the user data."
            );
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
