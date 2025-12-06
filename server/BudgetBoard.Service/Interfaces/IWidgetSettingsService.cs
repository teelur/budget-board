using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IWidgetSettingsService
{
    Task CreateWidgetSettingsAsync(
        Guid userGuid,
        IWidgetSettingsCreateRequest<NetWorthWidgetConfiguration> request
    );
    Task<IEnumerable<IWidgetResponse>> ReadWidgetSettingsAsync(Guid userID);
    Task UpdateWidgetSettingsAsync(
        Guid userID,
        IWidgetSettingsUpdateRequest<NetWorthWidgetConfiguration> request
    );
    Task DeleteWidgetSettingsAsync(Guid userID);
}
