namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Interface for syncing data from external providers.
/// </summary>
public interface ISyncProvider
{
    /// <summary>
    /// Refresh accounts from the external provider.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose accounts are being refreshed.</param>
    /// <returns>A list of error messages encountered during the refresh process, if any.</returns>
    Task<IList<string>> RefreshAccountsAsync(Guid userGuid);

    /// <summary>
    /// Sync transaction history from the external provider.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose transaction history is being synced.</param>
    /// <returns>A list of error messages encountered during the sync process, if any.</returns>
    Task<IList<string>> SyncTransactionHistoryAsync(Guid userGuid);
}
