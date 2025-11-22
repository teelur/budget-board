using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing user budgets.
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Creates new budget entries for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="budget">The collection of budget creation requests.</param>
    /// <param name="isCopy">Indicates if the budgets are being copied from another period.</param>
    Task CreateBudgetsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> budget, bool isCopy);

    /// <summary>
    /// Retrieves budgets for a specific month.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="monthDate">The date indicating the month to retrieve budgets for.</param>
    /// <returns>A collection of budget details.</returns>
    Task<IReadOnlyList<IBudgetResponse>> ReadBudgetsAsync(Guid userGuid, DateTime monthDate);

    /// <summary>
    /// Updates an existing budget entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="updatedBudget">The budget update details.</param>
    Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest updatedBudget);

    /// <summary>
    /// Deletes a budget entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="budgetGuid">The unique identifier of the budget to delete.</param>
    Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid);
}
