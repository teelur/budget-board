namespace BudgetBoard.Service.Interfaces;

using BudgetBoard.Service.Models;

/// <summary>
/// Service for managing Toshl connection data.
/// </summary>
public interface IToshlService
{
    /// <summary>
    /// Stores the Toshl personal access token for the current user.
    /// </summary>
    /// <param name="userGuid">The user identifier.</param>
    /// <param name="accessToken">The Toshl personal access token.</param>
    Task ConfigureAccessTokenAsync(Guid userGuid, string accessToken);

    /// <summary>
    /// Removes the Toshl personal access token for the current user.
    /// </summary>
    /// <param name="userGuid">The user identifier.</param>
    Task RemoveAccessTokenAsync(Guid userGuid);

    /// <summary>
    /// Synchronizes Toshl metadata for the current user.
    /// </summary>
    /// <param name="userGuid">The user identifier.</param>
    /// <param name="force">If true, bypasses the auto-sync interval check.</param>
    /// <param name="trackFullSyncProgress">
    /// If true, update persisted full-sync progress fields while syncing.
    /// </param>
    Task<IReadOnlyList<string>> SyncAsync(
        Guid userGuid,
        bool force = false,
        bool trackFullSyncProgress = false
    );

    /// <summary>
    /// Reads the available Toshl categories and tags with their current Budget Board mappings.
    /// </summary>
    /// <param name="userGuid">The user identifier.</param>
    Task<IToshlCategoryMappingsResponse> ReadCategoryMappingsAsync(Guid userGuid);

    /// <summary>
    /// Persists Toshl category and tag mappings and reapplies them to existing Toshl transactions.
    /// </summary>
    /// <param name="userGuid">The user identifier.</param>
    /// <param name="request">The mapping update request.</param>
    Task UpdateCategoryMappingsAsync(Guid userGuid, IToshlCategoryMappingsUpdateRequest request);
}
