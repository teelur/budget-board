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
        IAutomaticCategorizationRuleRequest rule
    )
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        try
        {
            _ = new System.Text.RegularExpressions.Regex(rule.CategorizationRule);
        }
        catch (System.Text.RegularExpressions.RegexParseException ex)
        {
            _logger.LogError(
                "Invalid regex in automatic categorization rule: {ExceptionMessage}",
                ex.Message
            );
            throw new BudgetBoardServiceException(
                "Invalid regex in automatic categorization rule."
            );
        }

        if (
            userData.AutomaticCategorizationRules.Any(r =>
                r.CategorizationRule == rule.CategorizationRule
            )
        )
        {
            _logger.LogError(
                "Attempt to create an automatic categorization rule that already exists."
            );
            throw new BudgetBoardServiceException(
                "An automatic categorization rule with this regex already exists."
            );
        }

        if (
            !TransactionCategoriesConstants.DefaultTransactionCategories.Any(c =>
                c.Value == rule.Category
            )
            && !userData.TransactionCategories.Any(c =>
                c.Value.Equals(rule.Category, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            _logger.LogError(
                "Attempt to create an automatic categorization rule with an invalid category."
            );
            throw new BudgetBoardServiceException("Invalid category provided.");
        }

        var newRule = new AutomaticCategorizationRule
        {
            ID = Guid.NewGuid(),
            UserID = userData.Id,
            CategorizationRule = rule.CategorizationRule,
            Category = rule.Category,
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
        return userData.AutomaticCategorizationRules.Select(
            r => new AutomaticCategorizationRuleResponse
            {
                ID = r.ID,
                CategorizationRule = r.CategorizationRule,
                Category = r.Category,
            }
        );
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
