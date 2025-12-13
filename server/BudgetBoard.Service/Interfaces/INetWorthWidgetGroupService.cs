using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing net worth widget groups.
/// </summary>
public interface INetWorthWidgetGroupService
{
    /// <summary>
    /// Reorders net worth widget groups for a user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user who owns the widget groups.</param>
    /// <param name="request">The reorder request containing the new order of the groups.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ReorderNetWorthWidgetGroupsAsync(
        Guid userGuid,
        INetWorthWidgetGroupReorderRequest request
    );
}
