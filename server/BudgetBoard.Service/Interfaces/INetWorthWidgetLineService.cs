using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
