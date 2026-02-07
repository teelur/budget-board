using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing net worth widget lines.
/// </summary>
public interface INetWorthWidgetLineService
{
    /// <summary>
    /// Creates a new line in the net worth widget configuration.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The net worth widget line creation details.</param>
    public Task CreateNetWorthWidgetLineAsync(
        Guid userGuid,
        INetWorthWidgetLineCreateRequest request
    );

    /// <summary>
    /// Updates an existing line in the net worth widget configuration.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The net worth widget line update details.</param>
    public Task UpdateNetWorthWidgetLineAsync(
        Guid userGuid,
        INetWorthWidgetLineUpdateRequest request
    );

    /// <summary>
    /// Deletes a line from the net worth widget configuration.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="widgetSettingsId">The unique identifier of the widget settings.</param>
    /// <param name="lineId">The unique identifier of the line to delete.</param>
    public Task DeleteNetWorthWidgetLineAsync(Guid userGuid, Guid widgetSettingsId, Guid lineId);

    /// <summary>
    /// Reorders the lines within a specified net worth widget group according to the provided requests.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">
    /// The reorder request containing the widget settings ID, group ID, and the new order of line IDs.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task ReorderNetWorthWidgetLinesAsync(
        Guid userGuid,
        INetWorthWidgetLineReorderRequest request
    );
}
