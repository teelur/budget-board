namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service interface for synchronizing financial data.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Synchronizes financial data for the specified user.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user.
    /// </param>
    /// <returns>
    /// A collection of messages or status updates regarding the sync process.
    /// </returns>
    Task<IReadOnlyList<string>> SyncAsync(Guid userGuid);
}
