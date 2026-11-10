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
    /// <param name="requests">The collection of budget creation requests.</param>
    /// <param name="autoManageParents">If true, parent budgets will be automatically created or updated; otherwise, they will not be managed.</param>
    Task CreateBudgetsAsync(
        Guid userGuid,
        IEnumerable<IBudgetCreateRequest> requests,
        bool autoManageParents = false
    );

    /// <summary>
    /// Retrieves budgets for a specific month.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="month">The date indicating the month to retrieve budgets for.</param>
    /// <returns>A collection of budget details.</returns>
    Task<IReadOnlyList<IBudgetResponse>> ReadBudgetsAsync(Guid userGuid, DateOnly month);

    /// <summary>
    /// Updates an existing budget entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The budget update details.</param>
    Task UpdateBudgetAsync(Guid userGuid, IBudgetUpdateRequest request);

    /// <summary>
    /// Deletes a budget entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="budgetGuid">The unique identifier of the budget to delete.</param>
    Task DeleteBudgetAsync(Guid userGuid, Guid budgetGuid);

    /// <summary>
    /// Exports the budget for a specific month as a structured template.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="month">The month to export.</param>
    /// <returns>A template containing hierarchical categories, limits, rollover flags, and icons.</returns>
    Task<BudgetTemplateExport> ExportBudgetAsync(Guid userGuid, DateOnly month);

    /// <summary>
    /// Imports a budget template into a specified month.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The import payload containing budget items and any new categories to create.</param>
    Task ImportBudgetAsync(Guid userGuid, BudgetImportRequest request);
}

