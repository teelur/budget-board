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
    /// <param name="request">The widget settings creation request for a Net Worth Widget.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateWidgetSettingsAsync(
        Guid userGuid,
        IWidgetSettingsCreateRequest<NetWorthWidgetConfiguration> request
    );

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
    /// <param name="request">The widget settings update request for a Net Worth Widget.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateWidgetSettingsAsync(
        Guid userGuid,
        IWidgetSettingsUpdateRequest<NetWorthWidgetConfiguration> request
    );

    /// <summary>
    /// Deletes widget settings for a specific user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteWidgetSettingsAsync(Guid userGuid, Guid widgetGuid);
}
