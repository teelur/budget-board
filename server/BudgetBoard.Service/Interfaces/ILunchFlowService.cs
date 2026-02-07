namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for syncing data with LunchFlow.
/// </summary>
public interface ILunchFlowService : ISyncProvider
{
    /// <summary>
    /// Configure the API key for LunchFlow.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose API key is being configured.</param>
    /// <param name="apiKey">The API key for the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ConfigureApiKeyAsync(Guid userGuid, string apiKey);

    /// <summary>
    /// Removes the API key for LunchFlow.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose API key is being removed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveApiKeyAsync(Guid userGuid);
}
