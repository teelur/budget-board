using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

public interface IUserSettingsService
{
    Task<IUserSettingsResponse> ReadUserSettingsAsync(Guid userGuid);
    Task UpdateUserSettingsAsync(
        Guid userGuid,
        IUserSettingsUpdateRequest userSettingsUpdateRequest
    );
}
