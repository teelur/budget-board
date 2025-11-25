using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing user-specific settings.
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    /// Retrieves the settings for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <returns>The user settings details.</returns>
    Task<IUserSettingsResponse> ReadUserSettingsAsync(Guid userGuid);

    /// <summary>
    /// Updates the settings for the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The user settings update details.</param>
    Task UpdateUserSettingsAsync(Guid userGuid, IUserSettingsUpdateRequest request);
}
