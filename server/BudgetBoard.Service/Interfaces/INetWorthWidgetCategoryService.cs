using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing net worth widget categories.
/// </summary>
public interface INetWorthWidgetCategoryService
{
    /// <summary>
    /// Creates a new net worth widget category.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CreateNetWorthWidgetCategoryAsync(
        Guid userGuid,
        INetWorthWidgetCategoryCreateRequest request
    );

    /// <summary>
    /// Updates an existing net worth widget category.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task UpdateNetWorthWidgetCategoryAsync(
        Guid userGuid,
        INetWorthWidgetCategoryUpdateRequest request
    );

    /// <summary>
    /// Deletes a net worth widget category for the specified user and widget settings.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user who owns the widget category.</param>
    /// <param name="widgetSettingsId">The unique identifier of the widget settings associated with the category.</param>
    /// <param name="lineId">The unique identifier of the line associated with the category.</param>
    /// <param name="categoryId">The unique identifier of the net worth widget category to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public Task DeleteNetWorthWidgetCategoryAsync(
        Guid userGuid,
        Guid widgetSettingsId,
        Guid lineId,
        Guid categoryId
    );
}
