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
    private readonly ILogger<IAutomaticRuleService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;
    private readonly ITransactionService _transactionService = transactionService;
    private readonly IStringLocalizer<ResponseStrings> _responseLocalizer = responseLocalizer;
    private readonly IStringLocalizer<LogStrings> _logLocalizer = logLocalizer;

    /// <inheritdoc />
    public async Task CreateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleCreateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        if (request.Conditions.Count == 0)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["NoConditionsCreateLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["NoConditionsCreateError"]);
        }

        if (request.Actions.Count == 0)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["NoActionsCreateLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["NoActionsCreateError"]);
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
                    ID = Guid.NewGuid(),
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
                    ID = Guid.NewGuid(),
                    Field = a.Field,
                    Operator = a.Operator,
                    Value = a.Value,
                    RuleID = newRuleId,
                }),
            ],
        };

        _userDataContext.AutomaticRules.Add(newRule);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IAutomaticRuleResponse>> ReadAutomaticRulesAsync(Guid userGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        return userData
            .AutomaticRules.Select(r => new AutomaticRuleResponse
            {
                ID = r.ID,
                Conditions = [.. r.Conditions.Select(c => new RuleParameterResponse(c))],
                Actions = [.. r.Actions.Select(a => new RuleParameterResponse(a))],
            })
            .ToList();
    }

    /// <inheritdoc />
    public async Task UpdateAutomaticRuleAsync(Guid userGuid, IAutomaticRuleUpdateRequest request)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var existingRule = userData.AutomaticRules.FirstOrDefault(r => r.ID == request.ID);

        if (existingRule == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AutomaticRuleNotFoundLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["AutomaticRuleNotFoundError"]);
        }

        if (request.Conditions.Count == 0)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["NoConditionsUpdateLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["NoConditionsUpdateError"]);
        }

        if (request.Actions.Count == 0)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["NoActionsUpdateLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["NoActionsUpdateError"]);
        }

        _userDataContext.RuleConditions.RemoveRange(existingRule.Conditions);
        foreach (var condition in request.Conditions)
        {
            _userDataContext.RuleConditions.Add(
                new RuleCondition
                {
                    Field = condition.Field,
                    Operator = condition.Operator,
                    Value = condition.Value,
                    RuleID = existingRule.ID,
                }
            );
        }

        _userDataContext.RuleActions.RemoveRange(existingRule.Actions);
        foreach (var action in request.Actions)
        {
            _userDataContext.RuleActions.Add(
                new RuleAction
                {
                    Field = action.Field,
                    Operator = action.Operator,
                    Value = action.Value,
                    RuleID = existingRule.ID,
                }
            );
        }

        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAutomaticRuleAsync(Guid userGuid, Guid ruleGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var rule = userData.AutomaticRules.FirstOrDefault(r => r.ID == ruleGuid);
        if (rule == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["AutomaticRuleDeleteNotFoundLog"]);
            throw new BudgetBoardServiceException(
                _responseLocalizer["AutomaticRuleDeleteNotFoundError"]
            );
        }

        userData.AutomaticRules.Remove(rule);
        await _userDataContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<string> RunAutomaticRuleAsync(
        Guid userGuid,
        IAutomaticRuleCreateRequest request
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        var customCategories = userData.TransactionCategories.Select(tc => new CategoryBase()
        {
            Value = tc.Value,
            Parent = tc.Parent,
        });

        var allCategories = TransactionCategoriesHelpers.GetAllTransactionCategories(
            customCategories,
            userData.UserSettings?.DisableBuiltInTransactionCategories ?? false
        );

        var matchedTransactions = GetMatchingTransactions(
            request.Conditions,
            userData.Accounts.SelectMany(a => a.Transactions),
            allCategories
        );

        var matchedTransactionsCount = matchedTransactions.Count;
        _logger.LogInformation(
            "{LogMessage}",
            _logLocalizer["RuleMatchedTransactionsLog", matchedTransactionsCount]
        );

        int updatedCount = await ApplyActionsToTransactions(
            request.Actions,
            matchedTransactions,
            allCategories,
            userData.Id
        );
        _logger.LogInformation(
            "{LogMessage}",
            _logLocalizer["RuleAppliedActionsLog", updatedCount]
        );

        return _responseLocalizer["RuleRunSummary", matchedTransactionsCount, updatedCount];
    }

    private List<Transaction> GetMatchingTransactions(
        IEnumerable<IRuleParameterRequest> conditions,
        IEnumerable<Transaction> transactions,
        IEnumerable<ICategory> allCategories
    )
    {
        var matchedTransactions = transactions.Where(t =>
            t.Deleted == null && !(t.Account?.HideTransactions ?? false)
        );

        foreach (var condition in conditions)
        {
            if (condition == null)
            {
                _logger.LogError("{LogMessage}", _logLocalizer["InvalidConditionLog"]);
                throw new BudgetBoardServiceException(_responseLocalizer["InvalidConditionError"]);
            }

            try
            {
                matchedTransactions = AutomaticRuleHelpers.FilterOnCondition(
                    condition,
                    matchedTransactions,
                    allCategories,
                    _responseLocalizer
                );
            }
            catch (BudgetBoardServiceException bbex)
            {
                _logger.LogError(bbex, "{LogMessage}", _logLocalizer["ErrorApplyingConditionLog"]);
                throw;
            }
        }

        return [.. matchedTransactions];
    }

    private async Task<int> ApplyActionsToTransactions(
        IEnumerable<IRuleParameterRequest> actions,
        IEnumerable<Transaction> transactions,
        IEnumerable<ICategory> allCategories,
        Guid userId
    )
    {
        int updatedCount = 0;
        foreach (var action in actions)
        {
            try
            {
                updatedCount += await AutomaticRuleHelpers.ApplyActionToTransactions(
                    action,
                    transactions,
                    allCategories,
                    _transactionService,
                    userId,
                    _responseLocalizer
                );
            }
            catch (BudgetBoardServiceException bbex)
            {
                _logger.LogError(
                    bbex,
                    "{LogMessage}",
                    _logLocalizer["ErrorApplyingActionLog", bbex.Message]
                );
                continue;
            }
        }
        return updatedCount;
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        ApplicationUser? foundUser;
        try
        {
            foundUser = await _userDataContext
                .ApplicationUsers.Include(u => u.AutomaticRules)
                .ThenInclude(r => r.Conditions)
                .Include(u => u.AutomaticRules)
                .ThenInclude(r => r.Actions)
                .Include(u => u.TransactionCategories)
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Transactions)
                .Include(u => u.UserSettings)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "{LogMessage}",
                _logLocalizer["UserDataRetrievalErrorLog", ex.Message]
            );
            throw new BudgetBoardServiceException(_responseLocalizer["UserDataRetrievalError"]);
        }

        if (foundUser == null)
        {
            _logger.LogError("{LogMessage}", _logLocalizer["InvalidUserLog"]);
            throw new BudgetBoardServiceException(_responseLocalizer["InvalidUserError"]);
        }

        return foundUser;
    }
}
