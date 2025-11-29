using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for managing asset value history.
/// </summary>
public interface IValueService
{
    /// <summary>
    /// Creates a new value entry for an asset.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The value creation details.</param>
    Task CreateValueAsync(Guid userGuid, IValueCreateRequest request);

    /// <summary>
    /// Retrieves value history for a specific asset.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="assetId">The unique identifier of the asset.</param>
    /// <returns>A collection of value entries.</returns>
    Task<IReadOnlyList<IValueResponse>> ReadValuesAsync(Guid userGuid, Guid assetId);

    /// <summary>
    /// Updates an existing value entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="request">The value update details.</param>
    Task UpdateValueAsync(Guid userGuid, IValueUpdateRequest request);

    /// <summary>
    /// Deletes (soft deletes) a value entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="valueGuid">The unique identifier of the value to delete.</param>
    Task DeleteValueAsync(Guid userGuid, Guid valueGuid);

    /// <summary>
    /// Restores a previously deleted value entry.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="valueGuid">The unique identifier of the value to restore.</param>
    Task RestoreValueAsync(Guid userGuid, Guid valueGuid);
}
