using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing user budgets.
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Creates new budget entries for the specified user, automatically managing parent budgets.
    /// </summary>
    /// <remarks>
    /// If a parent budget doesn't exist for a child budget, it will be created automatically.
    /// Otherwise, the parent budget is updated to include the new child budget.
    /// </remarks>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="requests">The collection of budget creation requests.</param>
    Task CreateBudgetsWithParentsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> requests);

    /// <summary>
    /// Creates new budget entries for the specified user.
    /// </summary>
    /// <remarks>
    /// This method does not automatically create or update parent budgets.
    /// </remarks>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="requests">The collection of budget creation requests.</param>
    Task CreateBudgetsAsync(Guid userGuid, IEnumerable<IBudgetCreateRequest> requests);

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
    Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest request);

    /// <summary>
    /// Deletes a budget entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="budgetGuid">The unique identifier of the budget to delete.</param>
    Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid);
}
