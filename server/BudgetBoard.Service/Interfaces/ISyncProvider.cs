namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Interface for syncing data from external providers.
/// </summary>
public interface ISyncProvider
{
    /// <summary>
    /// Update the local cache with data from the external provider.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose data is being updated.</param>
    /// <returns>A list of error messages encountered during the update process, if any.</returns>
    Task<IList<string>> UpdateDataAsync(Guid userGuid);

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
}
