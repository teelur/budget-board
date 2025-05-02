using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBoard.Service;

public class BudgetService(ILogger<IBudgetService> logger, UserDataContext userDataContext) : IBudgetService
{
    private readonly ILogger<IBudgetService> _logger = logger;
    private readonly UserDataContext _userDataContext = userDataContext;

    public async Task CreateBudgetsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> budgets)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());

        int newBudgetsCount = 0;

        foreach (var budget in budgets)
        {
            // Ignore duplicate categories in a given month
            if (userData.Budgets.Any((b) =>
            b.Date.Month == budget.Date.Month
            && b.Date.Year == budget.Date.Year
            && b.Category.Equals(budget.Category, StringComparison.CurrentCultureIgnoreCase)))
            {
                continue;
            }

            Budget newBudget = new()
            {
                Date = budget.Date,
                Category = budget.Category,
                Limit = budget.Limit,
                UserID = userData.Id
            };

            userData.Budgets.Add(newBudget);
            newBudgetsCount++;
        }

        await _userDataContext.SaveChangesAsync();

        if (newBudgetsCount == 0)
        {
            _logger.LogError("Attempt to create duplicate budgets.");
            throw new BudgetBoardServiceException("The budget(s) you are trying to create already exist.");
        }
    }

    public async Task<IEnumerable<IBudgetResponse>> ReadBudgetsAsync(Guid userGuid, DateTime date)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budgets = userData.Budgets
        .Where(b => b.Date.Month == date.Month && b.Date.Year == date.Year);

        return budgets.Select(b => new BudgetResponse(b));
    }

    public async Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest updatedBudget)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = userData.Budgets.SingleOrDefault(b => b.ID == updatedBudget.ID);
        if (budget == null)
        {
            _logger.LogError("Attempt to update budget that does not exist.");
            throw new BudgetBoardServiceException("The budget you are trying to update does not exist.");
        }

        budget.Limit = updatedBudget.Limit;

        await _userDataContext.SaveChangesAsync();
    }

    public async Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid)
    {
        var userData = await GetCurrentUserAsync(userGuid.ToString());
        var budget = userData.Budgets.SingleOrDefault(b => b.ID == budgetGuid);
        if (budget == null)
        {
            _logger.LogError("Attempt to delete budget that does not exist.");
            throw new BudgetBoardServiceException("The budget you are trying to delete does not exist.");
        }

        _userDataContext.Budgets.Remove(budget);
        await _userDataContext.SaveChangesAsync();
    }

    private async Task<ApplicationUser> GetCurrentUserAsync(string id)
    {
        List<ApplicationUser> users;
        ApplicationUser? foundUser;
        try
        {
            users = await _userDataContext.ApplicationUsers
                .Include(u => u.Budgets)
                .ToListAsync();
            foundUser = users.FirstOrDefault(u => u.Id == new Guid(id));
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occurred while retrieving the user data: {ExceptionMessage}", ex.Message);
            throw new BudgetBoardServiceException("An error occurred while retrieving the user data.");
        }

        if (foundUser == null)
        {
            _logger.LogError("Attempt to create an account for an invalid user.");
            throw new BudgetBoardServiceException("Provided user not found.");
        }

        return foundUser;
    }
}
