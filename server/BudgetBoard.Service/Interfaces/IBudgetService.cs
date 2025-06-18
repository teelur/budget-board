using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IBudgetService
{
    Task CreateBudgetsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> budget, bool isCopy);
    Task<IEnumerable<IBudgetResponse>> ReadBudgetsAsync(Guid userGuid, DateTime date);
    Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest updatedBudget);
    Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid);
}
