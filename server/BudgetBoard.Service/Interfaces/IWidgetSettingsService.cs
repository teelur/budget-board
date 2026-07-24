using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing widget settings for users.
/// </summary>
public interface IWidgetSettingsService
{
    /// <summary>
    /// Creates new widget settings for a user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The widget settings creation request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateWidgetSettingsAsync(Guid userGuid, IWidgetSettingsCreateRequest request);

    /// <summary>
    /// Retrieves all widget settings for a specific user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of widget settings.</returns>
    Task<IEnumerable<IWidgetResponse>> ReadWidgetSettingsAsync(Guid userGuid);

    /// <summary>
    /// Updates existing widget settings for a user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="requests">The collection of widget settings update requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateWidgetSettingsAsync(
        Guid userGuid,
        IEnumerable<IWidgetSettingsUpdateRequest> requests
    );

    /// <summary>
    /// Deletes widget settings for a specific user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteWidgetSettingsAsync(Guid userGuid, Guid widgetGuid);

    /// <summary>
    /// Resets the grid layout for small screens to match the large screen layout for all widgets of a given user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose small screen layout will be reset.</param>
    /// <returns>A task that represents the asynchronous reset operation.</returns>
    Task ResetSmallScreenToLargeScreenLayoutAsync(Guid userGuid);
}
