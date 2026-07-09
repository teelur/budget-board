using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class AutomaticRuleService(
    ILogger<IAutomaticRuleService> logger,
    UserDataContext userDataContext,
    ITransactionService transactionService,
    IStringLocalizer<ResponseStrings> responseLocalizer,
    IStringLocalizer<LogStrings> logLocalizer
) : IAutomaticRuleService
{
    /// <inheritdoc />
    public async Task CreateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);

        if (request.Conditions.Count == 0)
        {
            logger.LogError("{LogMessage}", logLocalizer["NoConditionsCreateLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["NoConditionsCreateError"]);
        }

        if (request.Actions.Count == 0)
        {
            logger.LogError("{LogMessage}", logLocalizer["NoActionsCreateLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["NoActionsCreateError"]);
        }

        var newRuleId = Guid.NewGuid();
        var newRule = new AutomaticRule
        {
            ID = newRuleId,
            UserID = userData.Id,
            Conditions =
            [
                .. request.Conditions.Select(c => new RuleCondition
                {
                    Field = c.Field,
                    Operator = c.Operator,
                    Value = c.Value,
                    RuleID = newRuleId,
                }),
            ],
            Actions =
            [
                .. request.Actions.Select(a => new RuleAction
                {
                    Field = a.Field,
                    Operator = a.Operator,
                    Value = a.Value,
                    RuleID = newRuleId,
                }),
            ],
        };

        userDataContext.AutomaticRules.Add(newRule);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAutomaticRuleResponse>> ReadAutomaticRulesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        return userData.AutomaticRules.Select(r => new AutomaticRuleResponse(r)).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var existingRule = GetAutomaticRuleById(userData, request.ID);

        if (request.Conditions.Count > 0)
        {
            userDataContext.RuleConditions.RemoveRange(existingRule.Conditions);
            foreach (var condition in request.Conditions)
            {
                userDataContext.RuleConditions.Add(
                    new RuleCondition
                    {
                        Field = condition.Field,
                        Operator = condition.Operator,
                        Value = condition.Value,
                        RuleID = existingRule.ID,
                    }
                );
            }
        }
        if (request.Actions.Count > 0)
        {
            userDataContext.RuleActions.RemoveRange(existingRule.Actions);
            foreach (var action in request.Actions)
            {
                userDataContext.RuleActions.Add(
                    new RuleAction
                    {
                        Field = action.Field,
                        Operator = action.Operator,
                        Value = action.Value,
                        RuleID = existingRule.ID,
                    }
                );
            }
        }

        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAutomaticRuleAsync(Guid userGuid, Guid ruleGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var rule = GetAutomaticRuleById(userData, ruleGuid);

        userData.AutomaticRules.Remove(rule);
        await userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<string> RunOneOffAutomaticRuleAsync(
        Guid userGuid,
        IAutomaticRuleCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        int updatedCount = await RunAutomaticRule(userData, request, allCategories);
        logger.LogInformation("{LogMessage}", logLocalizer["RuleAppliedActionsLog", updatedCount]);

        return responseLocalizer["RuleRunSummary", updatedCount];
    }

    /// <inheritdoc />
    public async Task RunSavedAutomaticRulesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid);
        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(userData);

        var rules = await ReadAutomaticRulesAsync(userGuid);
        var ruleRequests = rules.Select(r => new AutomaticRuleCreateRequest(r));

        foreach (var rule in ruleRequests)
        {
            int updatedCount = await RunAutomaticRule(userData, rule, allCategories);
            logger.LogInformation(
                "{LogMessage}",
                logLocalizer["RuleAppliedActionsLog", updatedCount]
            );
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(Guid id)
    {
        return await UserDataServiceHelper.GetCurrentUserAsync(
            userDataContext,
            logger,
            logLocalizer,
            responseLocalizer,
            id,
            users =>
                users
                    .Include(u => u.AutomaticRules)
                    .ThenInclude(r => r.Conditions)
                    .Include(u => u.AutomaticRules)
                    .ThenInclude(r => r.Actions)
                    .Include(u => u.TransactionCategories)
                    .Include(u => u.Accounts)
                    .ThenInclude(a => a.Transactions)
                    .Include(u => u.UserSettings)
        );
    }

    private AutomaticRule GetAutomaticRuleById(ApplicationUser userData, Guid ruleId)
    {
        var rule = userData.AutomaticRules.FirstOrDefault(r => r.ID == ruleId);
        if (rule == null)
        {
            logger.LogError("{LogMessage}", logLocalizer["AutomaticRuleNotFoundLog"]);
            throw new BudgetBoardServiceException(responseLocalizer["AutomaticRuleNotFoundError"]);
        }
        return rule;
    }

    private async Task<int> RunAutomaticRule(
        ApplicationUser userData,
        IAutomaticRuleCreateRequest rule,
        IEnumerable<ITransactionCategory> allCategories
    )
    {
        var matchedTransactions = GetMatchingTransactions(
            rule.Conditions,
            userData.Accounts.SelectMany(a => a.Transactions),
            allCategories
        );

        logger.LogInformation(
            "{LogMessage}",
            logLocalizer["RuleMatchedTransactionsLog", matchedTransactions.Count]
        );

        return await ApplyActionsToTransactions(
            rule.Actions,
            matchedTransactions,
            allCategories,
            userData.Id
        );
    }

    private List<Transaction> GetMatchingTransactions(
        IEnumerable<IRuleParameterRequest> conditions,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories
    )
    {
        var matchedTransactions = transactions.Where(t =>
            t.Deleted == null && !t.Account!.HideTransactions
        );

        foreach (var condition in conditions)
        {
            try
            {
                matchedTransactions = AutomaticRuleConditionHandler.FilterOnCondition(
                    condition,
                    matchedTransactions,
                    allCategories,
                    responseLocalizer
                );
            }
            catch (BudgetBoardServiceException bbex)
            {
                logger.LogError(bbex, "{LogMessage}", logLocalizer["ErrorApplyingConditionLog"]);
                throw;
            }
        }

        return [.. matchedTransactions];
    }

    private async Task<int> ApplyActionsToTransactions(
        IEnumerable<IRuleParameterRequest> actions,
        IEnumerable<Transaction> transactions,
        IEnumerable<ITransactionCategory> allCategories,
        Guid userId
    )
    {
        int updatedCount = 0;
        foreach (var action in actions)
        {
            try
            {
                updatedCount += await AutomaticRuleActionHandler.ApplyActionToTransactions(
                    action,
                    [.. transactions],
                    allCategories,
                    transactionService,
                    userId,
                    responseLocalizer
                );
            }
            catch (BudgetBoardServiceException bbex)
            {
                logger.LogError(
                    bbex,
                    "{LogMessage}",
                    logLocalizer["ErrorApplyingActionLog", bbex.Message]
                );
                continue;
            }
        }
        return updatedCount;
    }
}
