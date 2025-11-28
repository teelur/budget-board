using BudgetBoard.Database.Data;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Interface for syncing data from external providers.
/// </summary>
public interface ISyncProvider
{
    /// <summary>
    /// Sync data from the external provider.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user whose data is being synced.
    /// </param>
    /// <returns>
    /// A list of error messages encountered during the sync process, if any.
    /// </returns>
    Task<IList<string>> SyncDataAsync(Guid userGuid);

    /// <summary>
    /// Configure the access token for the external provider.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user whose access token is being configured.
    /// </param>
    /// <param name="token">
    /// The access token to be configured for the user.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task ConfigureAccessTokenAsync(Guid userGuid, string token);
}
