using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class AutomaticRuleService(
    ILogger<IAutomaticRuleService> logger,
    UserDataContext userDataContext,
    ITransactionService transactionService
) : IAutomaticRuleService
{
    private readonly ILogger<IAutomaticRuleService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly ITransactionService _transactionService = transactionService;

    public async Task CreateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest rule)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (rule.Conditions.Count == 0)
        {
            _logger.LogError("Attempt to create an automatic rule with no conditions.");
            throw new BudgetBoardServiceException(
                "At least one condition must be provided for the rule."
            );
        }

        if (rule.Actions.Count == 0)
        {
            _logger.LogError("Attempt to create an automatic rule with no actions.");
            throw new BudgetBoardServiceException(
                "At least one action must be provided for the rule."
            );
        }

        var newRule = new AutomaticRule
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
                }),
            ],
        };

        _userDataContext.AutomaticRules.Add(newRule);
        try
        {
            await _userDataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while saving the automatic rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while saving the automatic rule."
            );
        }
    }

    public async Task<IEnumerable<IAutomaticRuleResponse>> ReadAutomaticRulesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        return userData
            .AutomaticRules.Select(r => new AutomaticRuleResponse
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
                    }),
                ],
            })
            .ToList();
    }

    public async Task UpdateAutomaticRuleAsync(
        Guid userGuid,
        IAutomaticRuleUpdateRequest updatedRule
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var existingRule = userData.AutomaticRules.FirstOrDefault(r => r.ID == updatedRule.ID);

        if (existingRule == null)
        {
            _logger.LogError("Attempt to update an automatic rule that does not exist.");
            throw new BudgetBoardServiceException("Automatic rule not found.");
        }

        if (updatedRule.Conditions.Count == 0)
        {
            _logger.LogError("Attempt to update an automatic rule with no conditions.");
            throw new BudgetBoardServiceException(
                "At least one condition must be provided for the rule."
            );
        }

        if (updatedRule.Actions.Count == 0)
        {
            _logger.LogError("Attempt to update an automatic rule with no actions.");
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
                "An error occurred while updating the automatic rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while updating the automatic rule."
            );
        }
    }

    public async Task DeleteAutomaticRuleAsync(Guid userGuid, Guid ruleGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var rule = userData.AutomaticRules.FirstOrDefault(r => r.ID == ruleGuid);
        if (rule == null)
        {
            _logger.LogError("Attempt to delete an automatic rule that does not exist.");
            throw new BudgetBoardServiceException("Automatic rule not found.");
        }

        userData.AutomaticRules.Remove(rule);
        try
        {
            await _userDataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "An error occurred while deleting the automatic rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "An error occurred while deleting the automatic rule."
            );
        }
    }

    public async Task<string> RunAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest rule)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase()
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories = TransactionCategoriesConstants.DefaultTransactionCategories.Concat(
            customCategories
        );

        var matchedTransactions = userData
            .Accounts.SelectMany(a => a.Transactions)
            .Where(t => t.Deleted == null && !(t.Account?.HideTransactions ?? false));

        foreach (var condition in rule.Conditions)
        {
            if (condition == null)
            {
                _logger.LogError("Encountered null condition in automatic rule.");
                throw new BudgetBoardServiceException("Invalid condition in automatic rule.");
            }

            try
            {
                matchedTransactions = AutomaticRuleHelpers.FilterOnCondition(
                    condition,
                    matchedTransactions,
                    allCategories
                );
            }
            catch (BudgetBoardServiceException bbex)
            {
                _logger.LogError(bbex, "Error applying condition: {Message}", bbex.Message);
                throw;
            }
        }
        var matchedTransactionsCount = matchedTransactions.Count();
        _logger.LogInformation(
            "Rule matched {matchedTransactionsCount} transactions.",
            matchedTransactionsCount
        );

        int updatedCount = 0;

        foreach (var action in rule.Actions)
        {
            try
            {
                updatedCount += await AutomaticRuleHelpers.ApplyActionToTransactions(
                    action,
                    matchedTransactions,
                    allCategories,
                    _transactionService,
                    userData.Id
                );
            }
            catch (BudgetBoardServiceException bbex)
            {
                _logger.LogError(bbex, "Error applying action: {Message}", bbex.Message);
                continue;
            }
        }

        _logger.LogInformation(
            "Applied automatic rule, updated {UpdatedCount} transactions.",
            updatedCount
        );

        return $"Rule matched {matchedTransactionsCount} transactions and applied {updatedCount} changes.";
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext
                .ApplicationUsers.Include(u => u.AutomaticRules)
                .ThenInclude(r => r.Conditions)
                .Include(u => u.AutomaticRules)
                .ThenInclude(r => r.Actions)
                .Include(u => u.TransactionCategories)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
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
